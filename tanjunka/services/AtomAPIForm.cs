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
    // form for editing moveable type service options
    class AtomAPIForm : ServiceSettingsForm
    {
        // blogname
        // blogurl
        // username
        // password
        // imageURL
        // imagePath
        // xmlrpcURL
        // blogid = select
        // publish = bool
        // allowcomments = bool
        // allowtrackback = bool
        // encoding = select
        // usestylesheet = bool
        // stylesheet (url)
        // copystylesheet = bool
        //----
        // useforms = bool
        // bodyform
        // moreform

        public AtomAPIForm(BlogPhotoService bps) :
            base(bps, "settings-Atomapiform.html", "AtomAPI Settings")
        {
        }

        public AtomAPIForm(BlogPhotoService bps, string formURL, string title) :
            base(bps, formURL, title)
        {
        }

        public virtual bool AllowCustomAtomEndpointURL { get { return true; } }
        public virtual bool AllowCustomImagePath { get { return false; } }

        class addBlogs : ArrayListOperator
        {
            AtomAPIForm m_mtf;
            int             m_index;
            string          m_blogid;   // currently selected ID;

            public addBlogs(AtomAPIForm mtf, string blogid)
            {
                m_mtf = mtf;
                m_blogid = blogid;
                m_index = 0;
            }

            public override bool operation(Object ob)
            {
                AtomAPIService.blogInfo blogInfo = (AtomAPIService.blogInfo)ob;

                bool bSelected;

                if (m_blogid.Length > 0)
                {
                    bSelected = (string.Compare(m_blogid,blogInfo.Name) == 0);
                }
                else
                {
                    bSelected = (m_index == 0);
                }

                m_mtf.AddOption("blogid", blogInfo.Name, m_index, bSelected);
                m_index++;
                return true;
            }
        }

        private void UpdateBlogList ()
        {
            ClearSelect("blogid");

            ((AtomAPIService)m_bps).OpOnBlogs (new addBlogs(this, m_bps.GetEntry("blogid")));
        }

        protected override void privGetSetAll (bool bGetFromForm)
        {
            GetSetText("servicename", bGetFromForm);
//            GetSetText("blogurl",     bGetFromForm);
            GetSetText("username",    bGetFromForm);
            GetSetText("password",    bGetFromForm);
//            GetSetText("imageURL",    bGetFromForm);
            if (AllowCustomImagePath)
            {
                GetSetText("imagePath",   bGetFromForm);
            }
            if (AllowCustomAtomEndpointURL)
            {
                GetSetText("AtomEndpointURL",   bGetFromForm);
            }
            GetSetText("stylesheet",  bGetFromForm);

            if (bGetFromForm)
            {
                string blogid = GetValue("blogid");
                if (blogid.Length > 0)
                {
                    int blogNdx = int.Parse(blogid);
                    ((AtomAPIService)m_bps).SetBlogIDByIndex(blogNdx);
                }
            }
            else
            {
                UpdateBlogList();
            }
            // encoding = select
            // copystylesheet = bool
        }

        protected override void ClickSomething (int id)
        {
            switch (id)
            {
            case 10: // guess info
                break;
            case 11: // test xmlrpc
                break;
            case 12: // update blogid list
                {
                    string atomEndpointURL;

                    if (AllowCustomAtomEndpointURL)
                    {
                        atomEndpointURL = GetText("AtomEndpointURL");
                    }
                    else
                    {
                        atomEndpointURL = m_bps.GetEntry("AtomEndpointURL");
                    }
                    m_bps.UpdateBlogList(
                            GetText("username"),
                            GetText("password"),
                            atomEndpointURL);
                    UpdateBlogList();
                }
                break;
            default:
                base.ClickSomething(id);
                break;
            }
        }
    }
}