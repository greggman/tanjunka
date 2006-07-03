#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Net;
using System.Globalization;
using CookComputing.XmlRpc;
using Atomizer;

#endregion

namespace Tanjunka
{
    // concrete class to handle the moveabletype service
    [Serializable]
    public class AtomAPIService : BlogPhotoService
    {
        [Serializable]
        public enum atomServiceType
        {
            post,
            feed,
            firstFeed,
            prevFeed,
            nextFeed,
            upload,
            categories,
            edit,
            alternate,
            unknown
        }

        [Serializable]
        public class atomService
        {
            public atomServiceType srvType;
            public string name;
            public string postURL;
            public string homepageURL;

            public atomService(service srv)
            {
                switch (srv.srvType)
                {
                case serviceType.post: srvType = atomServiceType.post; break;
                case serviceType.feed: srvType = atomServiceType.feed; break;
                case serviceType.firstFeed: srvType = atomServiceType.firstFeed; break;
                case serviceType.prevFeed: srvType = atomServiceType.prevFeed; break;
                case serviceType.nextFeed: srvType = atomServiceType.nextFeed; break;
                case serviceType.upload: srvType = atomServiceType.upload; break;
                case serviceType.categories: srvType = atomServiceType.categories; break;
                case serviceType.edit: srvType = atomServiceType.edit; break;
                case serviceType.alternate: srvType = atomServiceType.alternate; break;
                default: srvType = atomServiceType.unknown; break;
            }

                name = srv.name;
                postURL = srv.postURL;
                homepageURL = srv.homepageURL;
            }
        }

        [Serializable]
        public class blogInfo
        {
            public string Name = "";
            public Dictionary<atomServiceType, atomService> _points = new Dictionary<atomServiceType, atomService>();
            public blogInfo(string name)
            {
                Name = name;
            }

            public void AddService(service srv)
            {
                atomService newSrv = new atomService(srv);
                _points[newSrv.srvType] = newSrv;
            }

            public bool HasService(atomServiceType srvType)
            {
                return _points.ContainsKey(srvType);
            }

            public string GetServiceURL(atomServiceType srvType)
            {
                return _points[srvType].postURL;
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

        private Dictionary<string, blogInfo> m_blogs;
        private List<blogInfo> _blogList = new List<blogInfo>();    // this is only used for the settings form
        private List<catInfo> m_categories;
        private blogInfo _currentBlogInfo;

        private void AddBlogService(service srv)
        {
            if (!m_blogs.ContainsKey(srv.name))
            {
                blogInfo blog = new blogInfo(srv.name);
                m_blogs[srv.name] = blog;
                _blogList.Add(blog);
            }
            m_blogs[srv.name].AddService(srv);
        }

        private readonly string[] HTMLs = { "postbody", "postextra",  };
        private readonly string[] Encodables = { "postbody", "postextra", "title", "keywords", "excerpt" };

        public AtomAPIService()
        {
            m_blogs = new Dictionary<string, blogInfo>();
            m_categories = new List<catInfo>();
        }

        // deserialize constructor
        protected AtomAPIService(SerializationInfo info, StreamingContext context) : base (info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            m_blogs = genericInfo.GetValue<Dictionary<string, blogInfo>>("m_blogs");
            m_categories = genericInfo.GetValue<List<catInfo>>("m_categories");
            bool haveBlogInfo = genericInfo.GetValue<bool>("haveBlogInfo");
            if (haveBlogInfo)
            {
                _currentBlogInfo = genericInfo.GetValue<blogInfo>("_currentBlogInfo");
            }

            // add blogs to list
            foreach (KeyValuePair<string, blogInfo> de in m_blogs)
            {
                _blogList.Add(de.Value);
            }
        }

        public override void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);

            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("m_blogs", m_blogs);
            genericInfo.AddValue("m_categories", m_categories);
            bool haveBlogInfo = (_currentBlogInfo != null);
            genericInfo.AddValue("haveBlogInfo", haveBlogInfo);
            if (haveBlogInfo)
            {
                genericInfo.AddValue("_currentBlogInfo", _currentBlogInfo);
            }
        }

        public override string GetServiceTypeName() { return "AtomAPI"; }

        public override bool CanPostPhoto() { return false; }
        public override bool CanPostEntry() { return true; }
        public override string[] GetHTMLs() { return HTMLs; }
        public override string[] GetEncodables() { return Encodables; }

        private Atom CreateAtom(string endPointURL, string username, string password, bool bRequireBlog)
        {
            if (bRequireBlog && _currentBlogInfo == null)
            {
                throw new ApplicationException("no blog selected");
            }

            generatorType generator = new generatorType();
            generator.url = "http://tanjunka.com/";
            generator.Value = "Tanjunka";
            generator.version = Util.GetVersion();

            Log.Printn("AtomEndPointURL:" + endPointURL);

            return Atom.Create(
                            new Uri(endPointURL),
                            generator,
                            username,
                            password);
        }

        protected override bool privatePostEntry(EntryInfo entry, PostInfo postInfo, ref PostedEntryInfo pei)
        {
            Atom atom = CreateAtom(
                GetEntry("AtomEndpointURL"),
                GetEntry("username"),
                GetEntry("password"),
                true);

            InitDefaults(entry);

            // has this been posted before on this service?
            pei.postedID = entry.GetEntry (this, EID_POSTED_ID);
            if (!postInfo.bPrevPosted || pei.postedID.Length == 0)
            {
                if (!_currentBlogInfo.HasService(atomServiceType.post))
                {
                    throw new ApplicationException("blog does not support posting new entries");
                }

                Log.Printn("posting NEW entry");
                // new post
                entryType atomEntry = atom.PostBlogEntry(
                            _currentBlogInfo.GetServiceURL(atomServiceType.post),
                            entry.GetEntry(this, "posttitle"),
                            postInfo.GetEntry("postbody"),
                            entry.GetEntry(this, "atom_category")
                            );

                //find edit link
                foreach (linkType link in atomEntry.links)
                {
                    if (link.srvType.Equals(serviceType.edit))
                    {
                        pei.postedID  = link.href;
                    }
                }
                pei.postedURL = "**unknown**";
            }
            else
            {
                Log.Printn("posting OLD entry");
                // old post

                entryType atomEntry = atom.EditBlogEntry(
                            pei.postedID,
                            entry.GetEntry(this, "posttitle"),
                            postInfo.GetEntry("postbody"),
                            entry.GetEntry(this, "atom_category")
                            );

                // for MT we need to just use the old ones but other services
                // might update this
                pei.postedURL = entry.GetEntry(this, EID_POSTED_URL);
            }

            Log.Printn("postedID  : " + pei.postedID);
            Log.Printn("postedURL : " + pei.postedURL);

            return true;
        }

        public override bool OpenSettingsForm(IWin32Window win)
        {
            bool result = false;
            AtomAPIForm form = new AtomAPIForm(this);
            if (form.ShowDialog() == DialogResult.OK)
            {
                result = true;
            }
            form.Dispose();

            return result;
        }
        public override bool DisplayPostOptionsForm() { return false; }
        public override string GetOptionForm() { return "postoptions-AtomAPI.html"; }

        private void GetSetAll(EntryInfo entry, TanWebBrowser wb, bool bGetFromForm)
        {
        }

        private void InitDefaults(EntryInfo entry)
        {
        }

        private void UpdateCategoriesFromFormToEntry(EntryInfo entry, TanWebBrowser wb)
        {
            for (int ii = 0; ii < m_categories.Count; ++ii)
            {
                bool bMainChecked = (String.Compare(wb.GetCheck("categor1_" + ii), "1") == 0);

                if (bMainChecked)
                {
                    entry.PutEntry(this, "atom_category", m_categories[ii].m_id, true);
                }
            }
        }

        // get the category list from the blog
        private void GetCategories()
        {
            try {
                Atom atom = CreateAtom(
                    GetEntry("AtomEndpointURL"),
                    GetEntry("username"),
                    GetEntry("password"),
                    true);

                string[] categories = atom.GetCategories(
                    _currentBlogInfo.GetServiceURL(atomServiceType.categories)
                    );

                m_categories.Clear();
                foreach (string cat in categories)
                {
                    m_categories.Add(new catInfo(cat, cat));
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

        private void UpdateCategoriesFromEntryToForm(EntryInfo entry, TanWebBrowser wb)
        {
            wb.ClearList("postcategoriestbody");

            string maincat = entry.GetEntry(this, "atom_category");
            foreach (catInfo cat in m_categories)
            {
                bool bMainChecked = false;
                bool bChecked = false;

                if (maincat.Length > 0 && String.Compare(maincat, cat.m_id) == 0)
                {
                    bMainChecked = true;
                }

                AddCategory (wb, cat.m_name, bChecked, bMainChecked);
            }
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

        public override void UpdateBlogList(string username, string password, string endPointURL)
        {
            try
            {
                Atom atom = CreateAtom(endPointURL, username, password, false);

                service[] services = atom.GetServices();

                m_blogs.Clear();
                _blogList.Clear();
                _currentBlogInfo = null;
                foreach (service srv in services)
                {
                    AddBlogService(srv);
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
                Atom atom = CreateAtom(
                    GetEntry("AtomEndpointURL"),
                    GetEntry("username"),
                    GetEntry("password"),
                    true);

                if (!_currentBlogInfo.HasService(atomServiceType.feed))
                {
                    throw new ApplicationException("blog does not support getting old entry");
                }

                feedType feed = atom.GetFeed(_currentBlogInfo.GetServiceURL(atomServiceType.feed));

                ClearOldPosts();

                foreach (entryType entry in feed.entries)
                {
                    string postID = null;
                    foreach (linkType link in entry.links)
                    {
                        if (link.srvType.Equals(serviceType.edit))
                        {
                            postID = link.href;
                        }
                    }

                    if (postID != null)
                    {
                        DateTime created = System.DateTime.Now;

                        try
                        {
                            created = DateTime.Parse(entry.issued, DateTimeFormatInfo.InvariantInfo);
                        }
                        catch (Exception ex)
                        {
                            Log.Printn("entry.issue not correct format: " + ex.Message);
                        }

                        string author = "*unknown*";
                        string title =  "*uknonwn*";

                        if (entry.author != null && entry.author.name != null)
                        {
                            author = entry.author.name;
                        }

                        if (entry.title != null)
                        {
                            title = entry.title;
                        }

                        AddOldPost(
                            title,
                            author,
                            created,
                            postID);
                    }
                    else
                    {
                        Log.Printn("post " + entry.title + " has no edit link");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Localize.G.GetText("LabelUhOh"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void privGetPost (EntryInfo entry, ref PostedEntryInfo pei)
        {
            Atom atom = CreateAtom(
                GetEntry("AtomEndpointURL"),
                GetEntry("username"),
                GetEntry("password"),
                true);

            entryType atomEntry = atom.GetBlogEntry(pei.postedID);

            pei.postedURL = "**unknonwn**";

            DateTime created = DateTime.ParseExact(atomEntry.created, "yyyyMMddTHH:mm:ss", DateTimeFormatInfo.InvariantInfo);

            entry.PutEntry(this, "posttitle", atomEntry.title, false);
            entry.PutEntry(this, "postbody", atomEntry.contentValue.Text, false);
            entry.PutEntry(this, "postdate", Util.StringFromDate(created), false);
        }

        //--------------------- AtomAPI only -----------
        public void OpOnBlogs(ArrayListOperator alo)
        {
            foreach (Object ob in _blogList)
            {
                if (!alo.operation(ob))
                {
                    break;
                }
            }
        }

        public void SetBlogIDByIndex(int index)
        {
            _currentBlogInfo = _blogList[index];
        }
    }

    public class AtomAPIServiceCreator : BlogPhotoServiceCreator
    {
        public override BlogPhotoService GetNewService()
        {
            return new AtomAPIService();
        }
    }
}

