#region Using directives

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

using CookComputing.XmlRpc;

#endregion

namespace Tanjunka
{
    [Serializable]
    public class BloggerAPIService : BlogPhotoService
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

        private BloggerAPIProxy bapi;
        private List<blogInfo> m_blogs;
        private readonly string[] HTMLs = { "postbody", "postextra",  };
        private readonly string[] Encodables = { "postbody", "postextra", "title", };

        public BloggerAPIService()
        {
            bapi = new BloggerAPIProxy();
            bapi.KeepAlive = false;
            m_blogs = new List<blogInfo>();

            PutBoolEntry("publish", true);
        }

        // deserialize constructor
        protected BloggerAPIService(SerializationInfo info, StreamingContext context) : base (info, context)
        {
            bapi = new BloggerAPIProxy();
            bapi.KeepAlive = false;

            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
            m_blogs = genericInfo.GetValue<List<blogInfo>>("m_blogs");
        }

        public override void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);

            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("m_blogs", m_blogs); //using type inference
        }

        public override string GetServiceTypeName() { return "BloggerAPI"; }

        public override bool CanPostPhoto() { return false; }
        public override bool CanPostEntry() { return true; }
        public override string[] GetHTMLs() { return HTMLs; }
        public override string[] GetEncodables() { return Encodables; }

        protected override bool privatePostEntry(EntryInfo entry, PostInfo postInfo, ref PostedEntryInfo pei)
        {
            bapi.Url = GetEntry("xmlrpcURL");

            Log.Printn("xmlrpcURL:" + bapi.Url.ToString());

            string body = postInfo.GetEntry("postbody") + postInfo.GetEntry("postextra");

            // has this been posted before on this service?
            pei.postedID = entry.GetEntry (this, EID_POSTED_ID);
            if (!postInfo.bPrevPosted || pei.postedID.Length == 0)
            {
                Log.Printn("posting NEW entry");
                // new post
                pei.postedID = bapi.blogger_newPost(
                                            "appkey",   // appkey: needed?
                                            GetEntry("blogid"), //"5576229", //"10760381", // ,        // blogid: is what?
                                            GetEntry("username"),
                                            GetEntry("password"),
                                            body,
                                            postInfo.bPublish);

                // now, we have to get the permalink (or do we really care?)
                pei.postedURL = "**unknown**"; // TODO:

            }
            else
            {
                Log.Printn("posting OLD entry");
                // old post
                kludge_editPost(
                    "appkey",    // appkey: needed?
                    pei.postedID,
                    GetEntry("username"),
                    GetEntry("password"),
                    body,
                    (postInfo.bPrevPublished ? true : postInfo.bPublish)
                    );

                // for blogger we need to just use the old ones but other services
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
            BloggerAPIForm form = new BloggerAPIForm(this);
            if (form.ShowDialog() == DialogResult.OK)
            {
                result = true;
            }
            form.Dispose();

            return result;
        }
        public override bool DisplayPostOptionsForm() { return false; }
        public override string GetOptionForm() { return "postoptions-blogger.html"; }

        private void GetSetAll(EntryInfo entry, TanWebBrowser wb, bool bGetFromForm)
        {
            GetSetCustomCheck("publish", entry, wb, bGetFromForm);
        }

        private void InitDefaults(EntryInfo entry)
        {
            // is it set.  Is our default different?
            SetADefault(entry, "publish");
        }

        // get options from entry and put in form
        public override void updateEntryToOptions(PostSet ps, EntryInfo entry, TanWebBrowser wb)
        {
            InitDefaults(entry);
            GetSetAll (entry, wb, false);
            wb.SetInnerText("postoptionsname", this.GetName());
            wb.SetCheckBool("postset_enabled", ps.IsOn());
            wb.DisableElement("postoptions", !ps.IsOn());
        }

        public override void updateOptionsToEntry(PostSet ps, EntryInfo entry, TanWebBrowser wb)
        {
            GetSetAll (entry, wb, true);
            ps.SetOn(wb.GetCheckBool("postset_enabled"));
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
                bapi.Url = GetEntry("xmlrpcURL");

                blogger_RecentPost[] titles = bapi.blogger_getRecentPosts(
                    "appkey",
                    GetEntry("blogid"),
                    GetEntry("username"),
                    GetEntry("password"),
                    GetNumOldPosts() + 20
                    );

                ClearOldPosts();

                foreach (blogger_RecentPost title in titles)
                {
                    TanDictionary tan = AddOldPost(
                        GetBloggerTitleFromBody(title.content),
                        "", //title.userid,
                        title.dateCreated,
                        title.postid);

                    tan.PutEntry("blogger_content", title.content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Localize.G.GetText("LabelUhOh"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected virtual string GetBloggerTitleFromBody(string body)
        {
            string str = Util.StripHTML(body);
            return str.Substring(0, Math.Min(str.Length, 50)) + "...";
        }

        class findPost : ArrayListOperator
        {
            PostedEntryInfo m_pei;
            public TanDictionary m_post;

            public findPost(PostedEntryInfo pei)
            {
                m_pei = pei;
            }

            public override bool operation(Object ob)
            {
                TanDictionary td = ob as TanDictionary;

                if (m_pei.postedID == td.GetEntry("postid"))
                {
                    m_post = td;
                    return false;
                }

                return true;
            }
        }

        protected virtual string BloggerifyContent (EntryInfo entry, string content)
        {
            return content;
        }

        protected override void privGetPost (EntryInfo entry, ref PostedEntryInfo pei)
        {
            findPost fp = new findPost(pei);
            OpOnPosts(fp);

            pei.postedURL = "**unknown**"; // TODO:

            entry.PutEntry("postbody", BloggerifyContent(entry, fp.m_post.GetEntry("blogger_content")));
            entry.PutEntry("postdate", fp.m_post.GetEntry("date"));
        }

        //--------------------- blogger only -----------
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

        //--------------- blogger API kludge stuff -------------
        protected virtual bool kludge_editPost(string appkey, string postid, string username, string password, string content, bool publish)
        {
            return bapi.blogger_editPost(appkey, postid, username, password, content, publish);
        }

    }

    public class BloggerAPIServiceCreator : BlogPhotoServiceCreator
    {
        public override BlogPhotoService GetNewService()
        {
            return new BloggerAPIService();
        }
    }
}

