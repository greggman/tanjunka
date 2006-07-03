#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

#endregion

namespace Tanjunka
{
    // form for editing a post set.
    // a post set is a pair of services.  One for posting entries, one for posting photos
    // for example
    //
    // --------Entry Service---------------------Post Service
    //  Set 1: Gman's Movabletype Blog           GMan's Movabletype Blog
    //  Set 2: Yuki's Movabletype Blog           Yuki's Flickr
    //  Set 3: Yuki's Blogger                    Yuki's Flickr

    partial class PostSetForm : Form
    {
        PostSet         m_postSet;
        private PersistWindowState m_windowState;

        public PostSetForm(PostSet postSet)
        {
            m_postSet = postSet;

            InitializeComponent();
            m_windowState = new PersistWindowState(this);
        }

        private void PostSetForm_Load(object sender, EventArgs e)
        {
            Util.SetFormHTML(this.webBrowser1, "tan-postsetform.html");
        }

        private void SetText (string textid, string str)
        {
            Object[] args = new Object[2] { textid, str};
            this.webBrowser1.Document.InvokeScript("settext", args);
        }

        private string GetValue (string textid)
        {
            Object[] args = new Object[1] { textid};
            return this.webBrowser1.Document.InvokeScript("getvalue", args).ToString();
        }

        private void ClearSelect (string listname)
        {
            Object[] args = new Object[1] { listname, };
            this.webBrowser1.Document.InvokeScript("clearselect", args);
        }

        private void AddOption (string listname, string name, int id, bool selected)
        {
            Object[] args = new Object[4] { listname, name, id, selected};
            this.webBrowser1.Document.InvokeScript("addoption", args);
        }

        class addServices : ArrayListOperator
        {
            PostSetForm m_pf;
            PostSet     m_ps;
            int         m_index;

            public addServices(PostSetForm pf, PostSet ps)
            {
                m_pf = pf;
                m_ps = ps;
                m_index = 0;
            }

            public override bool operation(Object ob)
            {
                BlogPhotoService bps = (BlogPhotoService)ob;

                if (bps.CanPostPhoto())
                {
                    m_pf.AddOption("photoservice", bps.GetName(), m_index, m_ps.GetPhotoService() == bps);
                }
                if (bps.CanPostEntry())
                {
                    m_pf.AddOption("blogservice", bps.GetName(), m_index, m_ps.GetBlogService() == bps);
                }
                m_index++;
                return true;
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ClearSelect("blogservice");
            ClearSelect("photoservice");

            SetText("postsetname", m_postSet.GetName());
            UserSettings.G.OpOnBlogSettings(new addServices(this, m_postSet));
            AddOption("photoservice", "--NONE--", -1, m_postSet.GetPhotoService() == null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
//            int blogNdx  = int.Parse(this.webBrowser1.Document.All["blogservice"].GetAttribute("value"));
//            int photoNdx = int.Parse(this.webBrowser1.Document.All["photoservice"].GetAttribute("value"));
//            string name   = this.webBrowser1.Document.All["postsetname"].GetAttribute("value").ToString();
            int blogNdx  = int.Parse(GetValue("blogservice"));
            int photoNdx = int.Parse(GetValue("photoservice"));
            string name   = GetValue("postsetname");

            Log.Printn("name : " + name);
            Log.Printn("blogndx : " + blogNdx);
            Log.Printn("photondx : " + photoNdx);

            m_postSet.SetBlogService(UserSettings.G.GetBlogSettingsByIndex(blogNdx));
            if (photoNdx >= 0)
            {
                m_postSet.SetPhotoService(UserSettings.G.GetBlogSettingsByIndex(photoNdx));
            }
            else
            {
                m_postSet.SetPhotoService(null);
            }
            m_postSet.SetName(name);

            this.DialogResult = DialogResult.OK;
        }
    }
}