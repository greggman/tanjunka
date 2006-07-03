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

#endregion

namespace Tanjunka
{
    // concrete class to handle the moveabletype service
    [Serializable]
    public class GManService : BlogPhotoService
    {
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

        private GManAPIProxy mapi;
        private List<catInfo> m_categories;
        private List<catInfo> m_icons;

        private readonly string[] HTMLs = { "postbody", "postextra",  };
        private readonly string[] Encodables = { "postbody", "postextra", "title", "keywords", "excerpt" };

        public GManService()
        {
            mapi = new GManAPIProxy();
            mapi.KeepAlive = false;
            m_categories = new List<catInfo>();
            m_icons = new List<catInfo>();
        }

        // deserialize constructor
        protected GManService(SerializationInfo info, StreamingContext context) : base (info, context)
        {
            mapi = new GManAPIProxy();
            mapi.KeepAlive = false;

            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            m_categories = genericInfo.GetValue<List<catInfo>>("m_categories");
            m_icons = genericInfo.GetValue<List<catInfo>>("m_icons");
        }

        public override void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);

            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("m_categories", m_categories);
            genericInfo.AddValue("m_icons", m_icons);
        }

        public override string GetServiceTypeName() { return "GMan API"; }

        public override bool CanPostPhoto() { return true; }
        public override bool CanPostEntry() { return true; }
        public override string[] GetHTMLs() { return HTMLs; }
        public override bool CanEncode() { return false; }
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
        }

        protected override bool privatePostEntry(EntryInfo entry, PostInfo postInfo, ref PostedEntryInfo pei)
        {
            InitDefaults(entry);

            mapi.Url = GetEntry("xmlrpcURL");

            mapi.XmlEncoding = Encoding.GetEncoding(65001); // unicode

            Log.Printn("xmlrpcURL:" + mapi.Url.ToString());

            Dictionary<string, string> hash = new Dictionary<string, string>();

            string body = postInfo.GetEntry("postbody");
            if (postInfo.GetEntry("postextra").Length > 100)
            {
                Regex r = Util.MakeRegex("(?:<gman_cuthere>|&lt;gman_cuthere&gt;)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Match m = r.Match(body);

                body = body +
                    (m.Success ? "" : "<gman_cuthere>") +
                    postInfo.GetEntry("postextra");
            }

            hash["title"]    = entry.GetEntry(this, "posttitle");
            hash["body"]     = body;
            hash["icon"]     = entry.GetEntry(this, "gman-icon");
            hash["category"] = entry.GetEntry(this, "gman-cat");
            hash["link"]     = entry.GetEntry(this, "gman-link");
            if (entry.GetEntry(this, "postdate").Length > 10)
            {
                hash["date-created"] = Util.DateFromString(entry.GetEntry(this, "postdate"))
                                       .ToString("yyyyMMddTHH:mm:ss",
                                       DateTimeFormatInfo.InvariantInfo);
            }
            hash["option-nocut"] = entry.GetEntry(this, "option-nocut");
            hash["option-notfrontfeature"] = entry.GetEntry(this, "option-notfrontfeature");
            hash["option-notheadline"]     = entry.GetEntry(this, "option-notheadline");
            hash["option-preformatted"]    = "1";
            hash["pingurls"]               = entry.GetEntry(this, "pingurls");

            gman_hash[] ghash = new gman_hash[hash.Count];
            gman_response response;

            {
                int ii = 0;
                foreach (KeyValuePair<string, string> de in hash)
                {
                    ghash[ii].id = de.Key;
                    ghash[ii].value = de.Value;
                    ii++;
                }
            }

            // has this been posted before on this service?
            pei.postedID = entry.GetEntry (this, EID_POSTED_ID);
            if (!postInfo.bPrevPosted || pei.postedID.Length == 0)
            {
                Log.Printn("posting NEW entry");
                // new post
                response = mapi.gman_newEntry(
                                            GetEntry("blogid"),
                                            GetEntry("username"),
                                            GetEntry("password"),
                                            ghash
                                            );

                if (response.status != 0)
                {
                    throw new ApplicationException(response.errMessage);
                }

                pei.postedID  = response.GetEntry("postid");
                pei.postedURL = response.GetEntry("posturl");
            }
            else
            {
                Log.Printn("posting OLD entry");
                // old post
                response = mapi.gman_editEntry(
                            pei.postedID,
                            GetEntry("username"),
                            GetEntry("password"),
                            ghash
                            );

                if (response.status != 0)
                {
                    throw new ApplicationException(response.errMessage);
                }

                // the post id might have changed
                pei.postedID  = response.GetEntry("postid");
                pei.postedURL = response.GetEntry("posturl");
            }

            SetCategories (entry, pei.postedID);

            Log.Printn("postedID  : " + pei.postedID);
            Log.Printn("postedURL : " + pei.postedURL);

            return true;
        }

        public override bool OpenSettingsForm(IWin32Window win)
        {
            bool result = false;
            GManForm GManForm = new GManForm(this);
            if (GManForm.ShowDialog() == DialogResult.OK)
            {
                result = true;
            }
            GManForm.Dispose();

            return result;
        }
        public override bool DisplayPostOptionsForm() { return false; }
        public override string GetOptionForm() { return "postoptions-GMan.html"; }

        private void GetSetAll(EntryInfo entry, TanWebBrowser wb, bool bGetFromForm)
        {
            GetSetSharedText("postdate", entry, wb, bGetFromForm);
            GetSetSharedText("pingurls", entry, wb, bGetFromForm);
            GetSetCustomCheck("option-nocut", entry, wb, bGetFromForm);
            GetSetCustomCheck("option-notfrontfeature", entry, wb, bGetFromForm);
            GetSetCustomCheck("option-notheadline", entry, wb, bGetFromForm);
        }

        private void InitDefaults(EntryInfo entry)
        {
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

                mt_category[] icons = mapi.gman_getIconList(
                    GetEntry("blogid"),
                    GetEntry("username"),
                    GetEntry("password")
                    );

                m_icons.Clear();
                foreach (mt_category cat in icons)
                {
                    m_icons.Add(new catInfo(cat.categoryId, cat.categoryName));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Localize.G.GetText("ErrorGetCategoryList") + "\n" + ex.Message);
            }

        }

        private void UpdateCategoriesFromFormToEntry(EntryInfo entry, TanWebBrowser wb)
        {
            {
                int id = Util.SafeIntParse(wb.GetValue("gman-cat"));

                if (id < m_categories.Count)
                {
                    entry.PutEntry(this, "gman-cat", m_categories[id].m_id, true);
                }
            }
            {
                int id = Util.SafeIntParse(wb.GetValue("gman-icon"));

                if (id < m_icons.Count)
                {
                    entry.PutEntry(this, "gman-icon", m_icons[id].m_name, true);
                }
            }
        }

        private void AddIcon(TanWebBrowser wb, bool bChecked, string name, string url, int id)
        {
            Object[] args = new Object[] { bChecked, name, url, id, };
            wb.Document.InvokeScript("addicon", args);
        }

        private void UpdateCategoriesFromEntryToForm(EntryInfo entry, TanWebBrowser wb)
        {
            wb.ClearList("gman-cat");
            wb.ClearList("gman-icon");
            wb.ClearList("gmanicontbody");

            {
                int id = 0;
                string maincat = entry.GetEntry(this, "gman-cat");
                foreach (catInfo cat in m_categories)
                {
                    bool bChecked = (String.Compare(maincat, cat.m_id) == 0);
                    wb.AddOption("gman-cat", cat.m_name, id, bChecked);
                    id++;
                }
            }
            {
                int id = 0;
                string maincat = entry.GetEntry(this, "gman-icon");
                foreach (catInfo cat in m_icons)
                {
                    bool bChecked = (String.Compare(maincat, cat.m_name) == 0);
                    wb.AddOption("gman-icon", cat.m_name, id, bChecked);
                    AddIcon(wb, bChecked, cat.m_name, cat.m_id, id);
                    id++;
                }
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

        protected override void privGetPost (EntryInfo entry, ref PostedEntryInfo pei)
        {
            mapi.Url = GetEntry("xmlrpcURL");

            gman_response response = mapi.gman_getEntry(
                            pei.postedID,
                            GetEntry("username"),
                            GetEntry("password"));

            if (response.status != 0)
            {
                throw new ApplicationException(response.errMessage);
            }

            pei.postedURL = response.GetEntry("postURL");

            DateTime dt = DateTime.ParseExact(response.GetEntry("date-created"), "yyyyMMddTHH:mm:ss", DateTimeFormatInfo.InvariantInfo);

            entry.PutEntry(this, "posttitle", response.GetEntry("title"), false);
            entry.PutEntry(this, "postbody",  response.GetEntry("body"), false);
            entry.PutEntry(this, "postdate",  Util.StringFromDate(dt), false);
            entry.PutEntry(this, "gman-icon",  response.GetEntry("icon"), true);
            entry.PutEntry(this, "gman-cat",   response.GetEntry("category"), true);
            entry.PutEntry(this, "gman-link",  response.GetEntry("link"), true);
            entry.PutEntry(this, "option-nocut", response.GetEntry("option-nocut"), true);
            entry.PutEntry(this, "option-notfrontfeature", response.GetEntry("option-notfrontfeature"), true);
            entry.PutEntry(this, "option-notheadline", response.GetEntry("option-notheadline"), true);
            entry.PutEntry(this, "pingurls", response.GetEntry("pingurls"), false);
        }

    }

    public class GManServiceCreator : BlogPhotoServiceCreator
    {
        public override BlogPhotoService GetNewService()
        {
            return new GManService();
        }
    }
}

