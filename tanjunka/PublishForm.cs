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
    // this is the form for publishing
    // the idea WAS that you could publish to more than one blog
    // unfortunately I can't think of a WYSIWYG interface for that.
    [ComVisible(true)]
    public partial class PublishForm : Form
    {
        private PersistWindowState m_windowState;
        private Dictionary<string, BlogPhotoServiceCreator> m_services;

        public PublishForm(Dictionary<string, BlogPhotoServiceCreator> services)
        {
            m_services = services;
            InitializeComponent();
            m_windowState = new PersistWindowState(this);
        }

        private void PublishForm_Load(object sender, EventArgs e)
        {
            this.webBrowser1.ObjectForScripting = this;
            Util.SetFormHTML(this.webBrowser1, "tan-publishform.html");
        }

        private void ClearList ()
        {
            Object[] args = new Object[1] { "listtbody", };
            this.webBrowser1.Document.InvokeScript("clearlist", args);
        }

        private void AddItem(bool bChecked, string name, string blogService, string photoService)
        {
            Object[] args = new Object[4] { bChecked, name, blogService, photoService, };
            this.webBrowser1.Document.InvokeScript("additem", args);
        }

        class addServices : ArrayListOperator
        {
            PublishForm m_pf;

            public addServices(PublishForm pf)
            {
                m_pf = pf;
            }

            public override bool operation(Object ob)
            {
                PostSet ps = (PostSet)ob;

                m_pf.AddItem(ps.IsOn(), ps.GetName(), ps.GetBlogServiceName(), ps.GetPhotoServiceName());
                return true;
            }
        }

        private void RefreshList()
        {
            ClearList();

            UserSettings.G.OpOnPostSets(new addServices(this));

            // add post sets
            #if false
            AddItem (true, "my service", "gman mt", "gman mt");
            AddItem (true, "my service", "gman mt", "gman mt");
            AddItem (true, "my service", "gman mt", "gman mt");
            AddItem (true, "my service", "gman mt", "gman mt");
            #endif
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            RefreshList();
        }

        public void CheckSomething (int id, int index, int onoff)
        {
            Log.Printn("checked " + id + " : " + index + " : " + onoff);

            UserSettings.G.GetPostSetByIndex(index).SetOn(onoff != 0);
        }

        public void EditSomething (int id, int index)
        {
            Log.Printn("clicked " + id + " : " + index);

            PostSet ps = UserSettings.G.GetPostSetByIndex(index);
            switch (id)
            {
            case 0: // edit postset
                PostSetForm postSetForm = new PostSetForm(ps);
                if (postSetForm.ShowDialog(this) == DialogResult.OK)
                {
                    // if the user clicked OK then add it to the current list of blogPhotoServices
                    RefreshList();
                }
                postSetForm.Dispose();
                break;
            case 1: // edit blog service
                if (ps.GetBlogService().OpenSettingsForm(this))
                {
                    RefreshList();
                }
                break;
            case 2: // edit photo service
                if (ps.GetPhotoService() != null)
                {
                    if (ps.GetPhotoService().OpenSettingsForm(this))
                    {
                        RefreshList();
                    }
                }
                break;
            }
        }

        class EnumPostServices : ArrayListOperator
        {
            public List<int>    m_serviceIndices;
            int m_index;

            public EnumPostServices()
            {
                m_serviceIndices = new List<int>();
                m_index = 0;
            }

            public override bool operation(Object ob)
            {
                PostSet ps = (PostSet)ob;

                if (ps.IsOn())
                {
                    m_serviceIndices.Add(m_index);
                }
                m_index++;

                return true;
            }
        }


        public void ClickSomething (int id)
        {
            switch (id)
            {
            case 0: // publish
                //
                this.DialogResult = DialogResult.OK;
                break;
            case 1: // add
                {
                    if (UserSettings.G.GetNumBlogSettings() == 0)
                    {
                        if (!ServicesForm.AddService(this, m_services))
                        {
                            return;
                        }
                    }

                    {
                        // let the add a postset
                        PostSet postSet = new PostSet();

                        // default it to the last blog and photo service (it's new and we are assuming the user just added a new serivce)
                        for (int ii = UserSettings.G.GetNumBlogSettings() - 1; ii >= 0; --ii)
                        {
                            BlogPhotoService bps = UserSettings.G.GetBlogSettingsByIndex(ii);
                            if (postSet.GetBlogService() == null && bps.CanPostEntry())
                            {
                                postSet.SetBlogService(bps);
                            }
                            if (postSet.GetPhotoService() == null && bps.CanPostPhoto())
                            {
                                postSet.SetPhotoService(bps);
                            }
                        }

                        // we must have a blogservice
                        if (postSet.GetBlogService() == null)
                        {
                            MessageBox.Show(this, Localize.G.GetText("ErrorNoBlogServices"), Localize.G.GetText("LabelOops"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            return;
                        }

                        PostSetForm postSetForm = new PostSetForm(postSet);
                        if (postSetForm.ShowDialog(this) == DialogResult.OK)
                        {
                            // if the user clicked OK then add it to the current list of blogPhotoServices
                            UserSettings.G.AddPostSet(postSet);
                            RefreshList();
                        }
                        postSetForm.Dispose();
                    }
                }
                break;
            case 2: // delete
                {
                    EnumPostServices eps = new EnumPostServices();

                    UserSettings.G.OpOnPostSets(eps);

                    if (eps.m_serviceIndices.Count > 0)
                    {
                        string prompt = String.Format(Localize.G.GetText("PromptDeletePostsets"), eps.m_serviceIndices.Count);
                        if (MessageBox.Show(this, prompt, Localize.G.GetText("LabelConfirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            // delete em backward so the indicies stay the same
                            for (int ii = eps.m_serviceIndices.Count - 1; ii >= 0; --ii)
                            {
                                UserSettings.G.DeletePostSetByIndex(eps.m_serviceIndices[ii]);
                            }
                            RefreshList();
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, Localize.G.GetText("ErrorNoPostSetsSelected"), Localize.G.GetText("LabelOops"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
                break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}