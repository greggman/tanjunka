#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

#endregion

namespace Tanjunka
{
    // gives you a list of configured services and lets you add more
    //
    // A service is either a blog or photo service
    // and the info needed to login/post
    [ComVisible(true)]
    public partial class ServicesForm : Form
    {
        private Dictionary<string, BlogPhotoServiceCreator> m_services;
        private PersistWindowState m_windowState;

        public ServicesForm(Dictionary<string, BlogPhotoServiceCreator> services)
        {
            m_services = services;
            InitializeComponent();
            m_windowState = new PersistWindowState(this);
        }

        private void ServicesForm_Load(object sender, EventArgs e)
        {
            Util.SetFormHTML(this.webBrowser1, "tan-servicesform.html");
            this.webBrowser1.ObjectForScripting = this;

        }

        public void AddServiceToList(bool on, string name, string typename)
        {
            Object[] args = new Object[3] { on, name, typename };

            this.webBrowser1.Document.InvokeScript("addservice", args);
        }

        private void ClearServiceList()
        {
            Object[] args = new Object[1] { "servicelist" };
            this.webBrowser1.Document.InvokeScript("clearlist", args);
        }

        class addServices : ArrayListOperator
        {
            ServicesForm m_sf;

            public addServices(ServicesForm sf)
            {
                m_sf = sf;
            }

            public override bool operation(Object ob)
            {
                BlogPhotoService bps = (BlogPhotoService)ob;

                m_sf.AddServiceToList(false, bps.GetName(), bps.GetServiceTypeName());

                return true;
            }
        }

        private void RefreshServiceList()
        {
            ClearServiceList();

            UserSettings.G.OpOnBlogSettings(new addServices(this));

            #if false
            AddServiceToList(false, "my first service", "movabletype");
            AddServiceToList(false, "my 2nd <br> service", "movabletype");
            AddServiceToList(true, "my 3rd service", "flickr");
            #endif
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            RefreshServiceList();
        }

        public void EditSomething (int id, int index)
        {
            BlogPhotoService bps = UserSettings.G.GetBlogSettingsByIndex (index);

            if (bps.OpenSettingsForm(this))
            {
                RefreshServiceList();
            }
        }

        class EnumServices : ArrayListOperator
        {
            public List<int>    m_serviceIndices;
            public string m_inUse;
            int m_index;
            TanWebBrowser m_wb;

            public EnumServices(TanWebBrowser wb)
            {
                m_serviceIndices = new List<int>();
                m_index = 0;
                m_wb = wb;
                m_inUse = "";
            }

            public override bool operation(Object ob)
            {
                BlogPhotoService bps = (BlogPhotoService)ob;

                if (m_wb.GetCheckBool("service0_" + m_index))
                {
                    if (!UserSettings.G.BlogSettingInUse(bps))
                    {
                        m_serviceIndices.Add(m_index);
                    }
                    else
                    {
                        m_inUse += bps.GetName() + "\n";
                    }
                }
                m_index++;

                return true;
            }
        }

        public static bool AddService(IWin32Window win, Dictionary<string, BlogPhotoServiceCreator> services)
        {
            bool bSuccess = false;
            // let the user pick a service type
            ServiceTypeForm serviceTypeForm = new ServiceTypeForm(services);
            if (serviceTypeForm.ShowDialog(win) == DialogResult.OK)
            {
                Log.Printn("selected : (" + serviceTypeForm.GetSelectedService() + ")");

                // get the selected service
                BlogPhotoServiceCreator bpsc = services[serviceTypeForm.GetSelectedService()];

                // create a settings class for that service
                BlogPhotoService blogPhotoService = bpsc.GetNewService();

                // allow the user to fill it out
                if (blogPhotoService.OpenSettingsForm(win))
                {
                    UserSettings.G.AddBlogSettings (blogPhotoService);
                    bSuccess = true;

                    // create a postset for this service if it supports enteis
                    if (blogPhotoService.CanPostEntry())
                    {
                        PostSet postSet = new PostSet();

                        postSet.SetName(blogPhotoService.GetName());
                        postSet.SetBlogService(blogPhotoService);
                        if (blogPhotoService.CanPostPhoto())
                        {
                            postSet.SetPhotoService(blogPhotoService);
                        }
                        else
                        {
                            // default it to the last blog and photo service (it's new and we are assuming the user just added a new serivce)
                            for (int ii = UserSettings.G.GetNumBlogSettings() - 1; ii >= 0; --ii)
                            {
                                BlogPhotoService bps = UserSettings.G.GetBlogSettingsByIndex(ii);
                                if (postSet.GetPhotoService() == null && bps.CanPostPhoto())
                                {
                                    postSet.SetPhotoService(bps);
                                    break;
                                }
                            }
                        }
                        UserSettings.G.AddPostSet(postSet);
                    }
                }
            }
            serviceTypeForm.Dispose();

            return bSuccess;
        }

        public void ClickSomething (int id)
        {
            switch (id)
            {
            case 1: // add
                {
                    if (AddService(this, m_services))
                    {
                        // if the user clicked OK then add it to the current list of blogPhotoServices
                        RefreshServiceList();
                    }
                }
                break;
            case 2: // delete
                {
                    EnumServices es = new EnumServices(webBrowser1);

                    UserSettings.G.OpOnBlogSettings(es);

                    if (es.m_inUse.Length > 0)
                    {
                        MessageBox.Show(this, Localize.G.GetText("ErrorServiceInUse") +"\n\n" + es.m_inUse, Localize.G.GetText("LabelOops"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        if (es.m_serviceIndices.Count > 0)
                        {
                            string prompt = String.Format(Localize.G.GetText("PromptDeleteServices"), es.m_serviceIndices.Count);
                            if (MessageBox.Show(this, prompt, Localize.G.GetText("Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                // delete em backward so the indicies stay the same
                                for (int ii = es.m_serviceIndices.Count - 1; ii >= 0; --ii)
                                {
                                    UserSettings.G.DeleteBlogSettingByIndex(es.m_serviceIndices[ii]);
                                }
                                RefreshServiceList();
                            }

                        }
                        else
                        {
                            MessageBox.Show(this, Localize.G.GetText("ErrorNoServicesSelected"), Localize.G.GetText("LabelOops"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                    }
                }
                break;
            }
        }

        private void okay_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

    }
}