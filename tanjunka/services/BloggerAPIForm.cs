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
    class BloggerAPIForm : ServiceSettingsForm
    {
        public BloggerAPIForm(BlogPhotoService bps) :
            base(bps, "settings-BloggerAPIForm.html", "Blogger API Settings")
        {
        }


        public BloggerAPIForm(BlogPhotoService bps, string formFilename, string title) :
            base(bps, formFilename, title)
        {
        }

        class addBlogs : ArrayListOperator
        {
            BloggerAPIForm  m_bf;
            int             m_index;
            string          m_blogid;   // currently selected ID;

            public addBlogs(BloggerAPIForm bf, string blogid)
            {
                m_bf = bf;
                m_blogid = blogid;
                m_index = 0;
            }

            public override bool operation(Object ob)
            {
                BloggerAPIService.blogInfo blogInfo = (BloggerAPIService.blogInfo)ob;

                bool bSelected;

                if (m_blogid.Length > 0)
                {
                    bSelected = (string.Compare(m_blogid,blogInfo.m_id) == 0);
                }
                else
                {
                    bSelected = (m_index == 0);
                }

                m_bf.AddOption("blogid", blogInfo.m_name, m_index, bSelected);
                m_index++;
                return true;
            }
        }

        private void UpdateBlogList ()
        {
            ClearSelect("blogid");

            ((BloggerAPIService)m_bps).OpOnBlogs (new addBlogs(this, m_bps.GetEntry("blogid")));
        }

        // over
        protected virtual void GetSetExtra (bool bGetFromForm)
        {
            GetSetText("xmlrpcURL",   bGetFromForm);
        }

        protected override void privGetSetAll (bool bGetFromForm)
        {
            GetSetText("username",    bGetFromForm);
            GetSetText("password",    bGetFromForm);
            GetSetExtra(bGetFromForm);  // for bloggerservice and liveJournalService
            GetSetCheck("publish",         bGetFromForm);

            if (bGetFromForm)
            {
                string blogid = GetValue("blogid");
                if (blogid.Length > 0)
                {
                    int blogNdx = int.Parse(blogid);
                    ((BloggerAPIService)m_bps).SetBlogIDByIndex(blogNdx);
                }
            }
            else
            {
                UpdateBlogList();
            }
            // encoding = select
            // copystylesheet = bool
        }

        protected virtual string GetXMLRPCURL()
        {
            return GetText("xmlrpcURL");
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
                    m_bps.UpdateBlogList(
                            GetText("username"),
                            GetText("password"),
                            GetXMLRPCURL()  // because LiveJournal and Blogger don't have these in the form
                            );
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