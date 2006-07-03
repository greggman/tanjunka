#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Net;
using CookComputing.XmlRpc;

#endregion

namespace Tanjunka
{
    // concrete class to handle the moveabletype service
    [Serializable]
    public class MovabletypeService : BlogPhotoService
    {
        [Serializable]
        public class blogInfo
        {
            public string m_url;
            public string m_id;
            public string m_name;

            public blogInfo(string url, string id, string name)
            {
                m_url  = url;
                m_id   = id;
                m_name = name;
            }
        };

        [Serializable]
        public class catInfo
        {
            public string m_id;
            public string m_name;

            public catInfo(string id, string name)
            {
                m_id = id;
                m_name = name;
            }
        }

        private MovabletypeAPIProxy mapi;
        private List<blogInfo> m_blogs;
        private List<catInfo> m_categories;

        private readonly string[] HTMLs = { "postbody", "postextra",  };
        private readonly string[] Encodables = { "postbody", "postextra", "title", "keywords", "excerpt" };

        public MovabletypeService()
        {
            mapi = new MovabletypeAPIProxy();
            mapi.KeepAlive = false;
            m_blogs = new List<blogInfo>();
            m_categories = new List<catInfo>();

            PutBoolEntry("publish", true);
            PutBoolEntry("allowcomments", true);
            PutBoolEntry("allowtrackback", true);
        }

        // deserialize constructor
        protected MovabletypeService(SerializationInfo info, StreamingContext context) : base (info, context)
        {
            mapi = new MovabletypeAPIProxy();
            mapi.KeepAlive = false;

            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            m_blogs = genericInfo.GetValue<List<blogInfo>>("m_blogs");
            m_categories = genericInfo.GetValue<List<catInfo>>("m_categories");
        }

        public override void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);

            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("m_blogs", m_blogs); //using type inference
            genericInfo.AddValue("m_categories", m_categories); //using type inference
        }

        protected MovabletypeAPIProxy MAPI { get { return mapi; } }

        public override string GetServiceTypeName() { return "Movabletype API"; }

        public override bool CanPostPhoto() { return true; }
        public override bool CanPostEntry() { return true; }
        public override string[] GetHTMLs() { return HTMLs; }
        public override string[] GetEncodables() { return Encodables; }

        protected override bool privatePostPhoto(EntryInfo entry, TJPagePicture tjpic, ref PostedPhotoInfo ppi)
        {
            metaWeblog_file file;

            mapi.Url = GetEntry("xmlrpcURL");

            file.name = Util.CombineUnixPath(GetEntry("imagePath"), Path.GetFileName(tjpic.GetUploadName()));
            file.type = "image/jpeg";
            file.bits = Util.readFile(tjpic.GetTempPath());

            Log.Printn("uploading : (" + file.name + ") : size = " + file.bits.Length);
            Log.Printn("upload to : (" + mapi.Url + ")");

            metaWeblog_newObjInfo noi = mapi.metaWeblog_newMediaObject(
                    GetEntry("blogid"),
                    GetEntry("username"),
                    GetEntry("password"),
                    file);

            Log.Printn(noi.url);

            ppi.postedURL  = noi.url;
            ppi.postedSize.Width = tjpic.GetNewWidth();
            ppi.postedSize.Height = tjpic.GetNewHeight();
            ppi.largerURL  = noi.url;

            return true;
        }

        protected void SetCategories (EntryInfo entry, string postID)
        {
            string maincat = entry.GetEntry(this, "mt_main_category");
            List<string> selectedCats = Util.StringToStringList(entry.GetEntry(this, "mt_selected_categories"));

            mt_setCategory[] cats = new mt_setCategory[selectedCats.Count];

            int ii = 0;
            foreach (string catID in selectedCats)
            {
                cats[ii].categoryId = catID;
                cats[ii].isPrimary  = (String.Compare(maincat, catID) == 0);
                ++ii;
            }

            mapi.mt_setPostCategories(
                postID,
                GetEntry("username"),
                GetEntry("password"),
                cats
                );
        }

        protected override bool privatePostEntry(EntryInfo entry, PostInfo postInfo, ref PostedEntryInfo pei)
        {
            metaWeblog_newPostInfo npi = new metaWeblog_newPostInfo();

            InitDefaults(entry);

            mapi.Url = GetEntry("xmlrpcURL");
            int codepage = GetIntEntry("encoding");
            if (codepage == 0) { codepage = 65001; }

            mapi.XmlEncoding = Encoding.GetEncoding(codepage);

            Log.Printn("xmlrpcURL:" + mapi.Url.ToString());

            npi.title             = entry.GetEntry(this, "posttitle");
            npi.description       = postInfo.GetEntry("postbody");
            if (entry.GetEntry(this, "postdate").Length >= 8)
            {
                npi.dateCreated       = Util.DateFromString(entry.GetEntry(this, "postdate"));
            }
            else
            {
                npi.dateCreated       = System.DateTime.Now;
            }
            npi.mt_allow_comments = entry.GetIntEntry(this, "allowcomments");
            npi.mt_allow_pings    = entry.GetIntEntry(this, "allowtrackback");
            npi.mt_convert_breaks = ""; // "__default__";
            npi.mt_text_more      = postInfo.GetEntry("postextra");
            npi.mt_excerpt        = entry.GetEntry(this, "excerpt");
            npi.mt_keywords       = entry.GetEntry(this, "keywords");

            // convert url string into list
            npi.mt_tb_ping_urls = entry.GetEntry(this, "URLsToPing").Split(new Char [] {' ', ',', ';', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

            // has this been posted before on this service?
            pei.postedID = entry.GetEntry (this, EID_POSTED_ID);
            if (!postInfo.bPrevPosted || pei.postedID.Length == 0)
            {
                Log.Printn("posting NEW entry");
                // new post
                pei.postedID = mapi.metaWeblog_newPost(
                                            GetEntry("blogid"),
                                            GetEntry("username"),
                                            GetEntry("password"),
                                            npi,
                                            postInfo.bPublish
                                            );

                if (postInfo.bPublish)
                {
                    kludge_publish(
                        pei.postedID,
                        GetEntry("username"),
                        GetEntry("password")
                        );
                }

                // now, we have to get the permalink (or do we really care?)
                metaWeblog_postInfo mw_postInfo;

                mw_postInfo = mapi.metaWeblog_getPost(
                                pei.postedID,
                                GetEntry("username"),
                                GetEntry("password"));

                pei.postedURL = mw_postInfo.permaLink;
            }
            else
            {
                Log.Printn("posting OLD entry");
                // old post
                mapi.metaWeblog_editPost(
                            pei.postedID,
                            GetEntry("username"),
                            GetEntry("password"),
                            npi,
                            (postInfo.bPrevPublished ? false : postInfo.bPublish)
                            );

                if (postInfo.bPublish)
                {
                    mapi.mt_publish(
                        pei.postedID,
                        GetEntry("username"),
                        GetEntry("password")
                        );
                }

                // for MT we need to just use the old ones but other services
                // might update this
                pei.postedURL = entry.GetEntry(this, EID_POSTED_URL);
            }

            SetCategories (entry, pei.postedID);

            Log.Printn("postedID  : " + pei.postedID);
            Log.Printn("postedURL : " + pei.postedURL);

            return true;
        }

        public override bool OpenSettingsForm(IWin32Window win)
        {
            bool result = false;
            MovabletypeAPIForm movabletypeForm = new MovabletypeAPIForm(this);
            if (movabletypeForm.ShowDialog() == DialogResult.OK)
            {
                result = true;
            }
            movabletypeForm.Dispose();

            return result;
        }
        public override bool DisplayPostOptionsForm() { return false; }
        public override string GetOptionForm() { return "postoptions-movabletype.html"; }

        private void GetSetAll(EntryInfo entry, TanWebBrowser wb, bool bGetFromForm)
        {
            GetSetSharedText("excerpt", entry, wb, bGetFromForm);
            GetSetSharedText("keywords", entry, wb, bGetFromForm);
            GetSetSharedText("postdate", entry, wb, bGetFromForm);
            GetSetSharedText("pingurls", entry, wb, bGetFromForm);
            GetSetCustomCheck("publish", entry, wb, bGetFromForm);
            GetSetCustomCheck("allowcomments", entry, wb, bGetFromForm);
            GetSetCustomCheck("allowtrackback", entry, wb, bGetFromForm);
        }

        private void InitDefaults(EntryInfo entry)
        {
            // is it set.  Is our default different?
            SetADefault(entry, "publish");
            SetADefault(entry, "allowcomments");
            SetADefault(entry, "allowtrackback");
        }

        // get options from entry and put in form
        public override void updateEntryToOptions(PostSet ps, EntryInfo entry, TanWebBrowser wb)
        {
            InitDefaults(entry);
            GetSetAll (entry, wb, false);
            wb.SetInnerText("postoptionsname", this.GetName());
            UpdateCategoriesFromEntryToForm (entry, wb);
            wb.SetCheckBool("postset_enabled", ps.IsOn());
            wb.DisableElement("postoptions", !ps.IsOn());
        }

        public override void updateOptionsToEntry(PostSet ps, EntryInfo entry, TanWebBrowser wb)
        {
            GetSetAll (entry, wb, true);
            UpdateCategoriesFromFormToEntry(entry, wb);
            ps.SetOn(wb.GetCheckBool("postset_enabled"));
        }

        // get the category list from the blog
        private void GetCategories()
        {
            mapi.Url = GetEntry("xmlrpcURL");

            try {
                mt_category[] categories = mapi.mt_getCategoryList(
                    GetEntry("blogid"),
                    GetEntry("username"),
                    GetEntry("password")
                    );

                m_categories.Clear();
                foreach (mt_category cat in categories)
                {
                    m_categories.Add(new catInfo(cat.categoryId, cat.categoryName));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Localize.G.GetText("ErrorGetCategoryList") + "\n" + ex.Message);
            }

        }

        private void AddCategory (TanWebBrowser wb, string catname, bool bChecked, bool bMainChecked)
        {
            Object[] args = new Object[3] { catname, bChecked, bMainChecked };
            wb.Document.InvokeScript("addcategory", args);
        }

        private void UpdateCategoriesFromFormToEntry(EntryInfo entry, TanWebBrowser wb)
        {
            List<string> checkedCatIDs = new List<string>();

            for (int ii = 0; ii < m_categories.Count; ++ii)
            {
                bool bChecked     = (String.Compare(wb.GetCheck("categor2_" + ii), "1") == 0);
                bool bMainChecked = (String.Compare(wb.GetCheck("categor1_" + ii), "1") == 0);

                if (bChecked)
                {
                    checkedCatIDs.Add(m_categories[ii].m_id);
                }
                if (bMainChecked)
                {
                    entry.PutEntry(this, "mt_main_category", m_categories[ii].m_id, true);
                }
            }

            entry.PutEntry(this, "mt_selected_categories", Util.StringListToString(checkedCatIDs), true);
        }

        private void UpdateCategoriesFromEntryToForm(EntryInfo entry, TanWebBrowser wb)
        {
            wb.ClearList("postcategoriestbody");

            string maincat = entry.GetEntry(this, "mt_main_category");
            List<string> selectedCats = Util.StringToStringList(entry.GetEntry(this, "mt_selected_categories"));
            foreach (catInfo cat in m_categories)
            {
                bool bMainChecked = false;
                bool bChecked = false;

                if (maincat.Length > 0 && String.Compare(maincat, cat.m_id) == 0)
                {
                    bMainChecked = true;
                }
                if (Util.StringInStringList(cat.m_id, selectedCats))
                {
                    bChecked = true;
                }

                AddCategory (wb, cat.m_name, bChecked, bMainChecked);
            }
        }

        public override void OptionsFormCommand(EntryInfo entry, TanWebBrowser wb, int value)
        {
            switch (value)
            {
            case 1: // update categories
                GetCategories();
                UpdateCategoriesFromEntryToForm(entry, wb);
                break;
            }
        }

        public override void UpdateBlogList(string username, string password, string xmlrpcURL)
        {
            try
            {
                BloggerAPIProxy bapi;

                bapi = new BloggerAPIProxy();
                bapi.KeepAlive = false;
                bapi.Url = xmlrpcURL;

                blogger_userBlog[] blogs = bapi.blogger_getUsersBlogs(
                                            "appkey",
                                            username,
                                            password);

                m_blogs.Clear();
                foreach (blogger_userBlog blog in blogs)
                {
                    m_blogs.Add(new blogInfo(blog.url, blog.blogid, blog.blogName));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Localize.G.GetText("LabelUhOh"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void privGetMorePosts()
        {
            try
            {
                mapi.Url = GetEntry("xmlrpcURL");

                mt_title[] titles = mapi.mt_getRecentPostTitles(
                    GetEntry("blogid"),
                    GetEntry("username"),
                    GetEntry("password"),
                    GetNumOldPosts() + 20
                    );

                ClearOldPosts();

                foreach (mt_title title in titles)
                {
                    AddOldPost(
                        title.title,
                        "", //title.userid,
                        title.dateCreated,
                        title.postid);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Localize.G.GetText("LabelUhOh"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // if the original had a convert line breaks filter
        // then we need to change all the link breaks to something
        // tanjunka will understand since it is an html editor
        private string HTMLify (string str, string filter)
        {
            if (filter.CompareTo("__default__") == 0)
            {
                Regex r = Util.MakeRegex("\\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                return r.Replace(str, "<br />");
            }

            return str;
        }

        protected override void privGetPost (EntryInfo entry, ref PostedEntryInfo pei)
        {
            mapi.Url = GetEntry("xmlrpcURL");

            metaWeblog_postInfo mw_postInfo;

            mw_postInfo = mapi.metaWeblog_getPost(
                            pei.postedID,
                            GetEntry("username"),
                            GetEntry("password"));

            pei.postedURL = mw_postInfo.permaLink;

            entry.PutEntry(this, "posttitle", mw_postInfo.title, false);
            entry.PutEntry(this, "postbody", HTMLify(mw_postInfo.description, Util.SafeGetString(mw_postInfo.mt_convert_breaks)), false);
            entry.PutEntry(this, "postdate", Util.StringFromDate(mw_postInfo.dateCreated), false);
            entry.PutIntEntry(this, "allowcomments", mw_postInfo.mt_allow_comments, false);
            entry.PutIntEntry(this, "allowtrackback", mw_postInfo.mt_allow_pings, false);
            entry.PutEntry(this, "postextra", HTMLify(mw_postInfo.mt_text_more, Util.SafeGetString(mw_postInfo.mt_convert_breaks)), false);
            entry.PutEntry(this, "excerpt", mw_postInfo.mt_excerpt, false);
            entry.PutEntry(this, "keywords", Util.SafeGetString(mw_postInfo.mt_keywords), false);

            // get categories
            mt_postCategory[] cats = mapi.mt_getPostCategories(
                pei.postedID,
                GetEntry("username"),
                GetEntry("password"));

            List<string> catList = new List<string>();

            foreach (mt_postCategory cat in cats)
            {
                catList.Add(cat.categoryId);
                if (cat.isPrimary)
                {
                    entry.PutEntry(this, "mt_main_category", cat.categoryId, true);
                }
            }

            entry.PutEntry(this, "mt_selected_categories", Util.StringListToString(catList), true);

            // get pings
            mt_ping[] pings = mapi.mt_getTrackbackPings(pei.postedID);

            string pingStr = "";
            foreach (mt_ping ping in pings)
            {
                if (pingStr.Length > 0)
                {
                    pingStr += "\n";
                }
                pingStr += ping.pingURL;
            }
            entry.PutEntry(this, "URLsToPing", pingStr, false);
        }

        //--------------------- movabletype only -----------
        public void OpOnBlogs(ArrayListOperator alo)
        {
            foreach (Object ob in m_blogs)
            {
                if (!alo.operation(ob))
                {
                    break;
                }
            }
        }

        public void SetBlogIDByIndex(int index)
        {
            PutEntry("blogid", m_blogs[index].m_id);
        }

        //--------------- movabletype API kludge stuff -------------
        public virtual bool kludge_publish(string postid, string username, string password)
        {
            return mapi.mt_publish(postid, username, password);
        }


    }

    public class MovabletypeServiceCreator : BlogPhotoServiceCreator
    {
        public override BlogPhotoService GetNewService()
        {
            return new MovabletypeService();
        }
    }
}

