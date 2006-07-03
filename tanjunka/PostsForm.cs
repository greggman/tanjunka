#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

#endregion

namespace Tanjunka
{
    // form for downloading old post
    partial class PostsForm : TanHTMLForm
    {
        PostSet     m_curPS;
        EntryInfo   m_entry;

        public PostsForm(PostSet ps) :
            base("tan-postsform.html", Localize.G.GetText("PostsTitle"))
        {
            m_curPS = ps;
            m_entry = null;
        }

        public EntryInfo GetEntry()
        {
            return m_entry;
        }

        public PostSet GetPostSet()
        {
            return m_curPS;
        }

        class addPostSets : ArrayListOperator
        {
            PostsForm m_pf;
            PostSet m_curPS;
            int id;

            public addPostSets(PostsForm pf, PostSet curPS)
            {
                m_pf = pf;
                m_curPS = curPS;
                id   = 0;
            }

            public override bool operation(Object ob)
            {
                PostSet ps = ob as PostSet;

                m_pf.AddOption("selectlist", ps.GetName(), id, ps == m_curPS);
                ++id;
                return true;
            }
        }

        class addPosts : ArrayListOperator
        {
            PostsForm m_pf;

            public addPosts(PostsForm pf)
            {
                m_pf = pf;
            }

            public override bool operation(Object ob)
            {
                TanDictionary td = ob as TanDictionary;

                m_pf.AddItemSSS(false, td.GetEntry("title"), td.GetEntry("userid"), td.GetEntry("date"));

                return true;
            }
        }

        private void UpdatePostsList()
        {
            ClearItems("listtbody");
            m_curPS.GetBlogService().OpOnPosts(new addPosts(this));
        }

        protected override void GetSetAll (bool bGetFromForm)
        {
            if (!bGetFromForm)
            {
                // fill in post set list
                ClearSelect("selectlist");
                UserSettings.G.OpOnPostSets(new addPostSets(this, m_curPS));

                // fill in posts based on current post set list
                UpdatePostsList();
            }
        }

        protected override void ClickSomething (int id)
        {
            switch (id)
            {
            case 1:    // Update (get top 20)
                m_curPS.GetBlogService().UpdatePosts();
                UpdatePostsList();
                break;
            case 2:    // Delete (delete selected entries)
                break;
            case 3:    // Get More (get the next 20 entries)
                m_curPS.GetBlogService().GetMorePosts();
                UpdatePostsList();
                break;
            }
        }

        protected override void EditSomething (int id, int index)
        {
            if (id == 10) // select postset
            {
                m_curPS = UserSettings.G.GetPostSetByIndex(index);
                UpdatePostsList();
            }
            else // select post
            {
                try
                {
                    // get the post and exit
                    m_entry = m_curPS.GetBlogService().GetPost(index);
                    this.DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Localize.G.GetText("LabelUhOh"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

    }
}