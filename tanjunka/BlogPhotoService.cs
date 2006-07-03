#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.IO;

#endregion

namespace Tanjunka
{
    [Serializable]
    public class TanDictionary : ISerializable
    {
        Dictionary<string,string>        m_data;

        public TanDictionary()
        {
            m_data = new Dictionary<string, string>();
        }

        // deserialize constructor
        protected TanDictionary(SerializationInfo info, StreamingContext context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            m_data = genericInfo.GetValue<Dictionary<string,string>>("m_data");
        }

        public virtual void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("m_data", m_data);
        }

        public bool EntryExists(string dataname)
        {
            return m_data.ContainsKey(dataname);
        }

        public string GetEntry (string dataname)
        {
            if (m_data.ContainsKey(dataname))
            {
                return m_data[dataname];
            }
            return "";
        }

        public void PutEntry (string dataname, string data)
        {
            m_data[dataname] = data;
        }

        public int GetIntEntry (string dataname)
        {
            int value = 0;
            string valueStr = GetEntry (dataname);
            if (valueStr.Length > 0)
            {
                value = int.Parse(valueStr);
            }
            return value;
        }

        public void PutIntEntry (string dataname, int value)
        {
            PutEntry(dataname, value.ToString());
        }

        public bool GetBoolEntry (string dataname)
        {
            return GetIntEntry (dataname) != 0;
        }

        public void PutBoolEntry (string dataname, bool value)
        {
            PutIntEntry(dataname, value ? 1 : 0);
        }
    }

    // the abstract interface for a service to which
    // you can post entires and/or photos
    [Serializable]
    public abstract class BlogPhotoService : ISerializable
    {
        public const string BSID_SERVICE_ID     = "__id__";
        public const string PSID_PIC_URL        = "__picURL__";
        public const string PSID_PIC_WIDTH      = "__picWidth__";
        public const string PSID_PIC_HEIGHT     = "__picHeight__";
        public const string PSID_PIC_LARGER_URL = "__picLargerURL__";
        public const string PSID_PIC_VERSION    = "__picVersion__";
        public const string EID_POSTED_ID      = "__postedID__";
        public const string EID_POSTED_URL     = "__postedURL__";

        private readonly string[] HTMLs = {  };
        private readonly string[] Encodables = {  };

        Dictionary<string,string>        m_settingsData;
        List<TanDictionary>              m_oldPostsInfo;
        Dictionary<string,TanDictionary> m_usersInfo;   // indexed by userid

        static int m_topID = 1;

        public class PostedPhotoInfo
        {
            public string postedURL;    // url of uploaded photo
            public Size postedSize;     // size the image ended up being
            public string largerURL;    // url of larger version if one exists

            public PostedPhotoInfo()
            {
                postedSize = new Size();
            }
        }

        public class PostedEntryInfo
        {
            public string postedID;
            public string postedURL;
        }

        public BlogPhotoService()
        {
            m_settingsData = new Dictionary<string,string>();
            m_oldPostsInfo = new List<TanDictionary>();
            m_usersInfo    = new Dictionary<string, TanDictionary>();
            //todo
            PutEntry (BSID_SERVICE_ID, "service_id_" + m_topID.ToString());
            m_topID++;
        }

        // deserialize constructor
        protected BlogPhotoService(SerializationInfo info, StreamingContext context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            m_settingsData = genericInfo.GetValue<Dictionary<string,string>>("m_settingsData");
            m_topID = genericInfo.GetValue<int>("m_topID");

            if (UserSettings.GetLoadVersion() >= 4)
            {
                m_oldPostsInfo = genericInfo.GetValue<List<TanDictionary>>("m_oldPostsInfo");
                m_usersInfo    = genericInfo.GetValue<Dictionary<string, TanDictionary>>("m_usersInfo");
            }
            else
            {
                m_oldPostsInfo = new List<TanDictionary>();
                m_usersInfo    = new Dictionary<string, TanDictionary>();
            }
        }

        public virtual void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("m_settingsData", m_settingsData);
            genericInfo.AddValue("m_topID", m_topID);
            genericInfo.AddValue("m_oldPostsInfo", m_oldPostsInfo);
            genericInfo.AddValue("m_usersInfo", m_usersInfo);
        }

        public string GetName()
        {
            return GetEntry("servicename");
        }

        public string GetID()
        {
            return GetEntry(BSID_SERVICE_ID);
        }

        public abstract string GetServiceTypeName();

        public virtual bool CanPostPhoto() { return false; }
        public virtual bool CanPostEntry() { return false; }
        public virtual bool CanEncode() { return true; }
        public virtual string[] GetHTMLs() { return HTMLs; }
        public virtual string[] GetEncodables() { return Encodables; }

        public virtual void Startup() { } // give this service a chance to allocate some resources for posting
        public virtual void Shutdown() { } // give this service a chance to free resoruces needed for posting

        public virtual bool HaveRoomFor(long numPhotos, long totalBytes) { return true; }

        public bool PostPhoto(EntryInfo entry, TJPagePicture tjpic)
        {
            PostedPhotoInfo ppi = new PostedPhotoInfo();

            if (privatePostPhoto (entry, tjpic, ref ppi))
            {
                tjpic.PutPhotoEntry (this,    PSID_PIC_URL       , ppi.postedURL);
                tjpic.PutIntPhotoEntry (this, PSID_PIC_WIDTH     , ppi.postedSize.Width);
                tjpic.PutIntPhotoEntry (this, PSID_PIC_HEIGHT    , ppi.postedSize.Height);
                tjpic.PutPhotoEntry (this,    PSID_PIC_LARGER_URL, ppi.largerURL);
                tjpic.PutIntPhotoEntry (this, PSID_PIC_VERSION   , tjpic.GetVersion());
            }

            return true;
        }

        protected virtual bool privatePostPhoto(EntryInfo entry, TJPagePicture tjpic, ref PostedPhotoInfo ppi)
        {
            #if false
            ppi.postedURL = "http://myfuncy.com/pic.jpg";
            ppi.largerURL = "http://myfuncy.com/pic-large.jpg";
            ppi.postedSize.Width = 123;
            ppi.postedSize.Height = 234;
            #endif
            return false;
        }

        public bool PostEntry(EntryInfo entry, PostInfo postInfo)
        {
            PostedEntryInfo pei = new PostedEntryInfo();

            postInfo.bPublish       = entry.GetBoolEntry(this, "publish");
            postInfo.bPrevPosted    = (entry.GetIntEntry("postCount") == entry.GetIntEntry(this, "previouslyPostedCount"));
            postInfo.bPrevPublished = (entry.GetIntEntry("postCount") == entry.GetIntEntry(this, "previouslyPublishedCount"));

            if (privatePostEntry(entry, postInfo, ref pei))
            {
                entry.PutEntry (this, EID_POSTED_ID , pei.postedID, true);
                entry.PutEntry (this, EID_POSTED_URL, pei.postedURL, true);

                if (!postInfo.bPrevPosted)
                {
                    entry.PutIntEntry(this, "previouslyPostedCount",  entry.GetIntEntry("postCount"), true);
                }
                if (postInfo.bPublish)
                {
                    entry.PutIntEntry(this, "previouslyPublishedCount",  entry.GetIntEntry("postCount"), true);
                }

                return true;
            }
            return false;
        }

        protected virtual bool privatePostEntry(EntryInfo entry, PostInfo postInfo, ref PostedEntryInfo pei)
        {
            return false;
        }

        public abstract bool OpenSettingsForm(IWin32Window win);
        public abstract bool DisplayPostOptionsForm();
        public virtual string GetOptionForm() { return "tan-nooptions.html"; }
        public virtual void OptionsFormCommand(EntryInfo entry, TanWebBrowser wb, int value) { }
        public virtual string FilterImageTags (EntryInfo entry, string str) { return str; }

        // get options from form and put in entry
        public virtual void updateOptionsToEntry(PostSet ps, EntryInfo entry, TanWebBrowser wb) { }

        // get options from entry and put in form
        public virtual void updateEntryToOptions(PostSet ps, EntryInfo entry, TanWebBrowser wb) { }

        // we have to pass in username, password and xmlrpcURL because they are not
        // set in the service when this is called.  The user may still be setting it up
        public virtual void UpdateBlogList(string username, string password, string xmlrpcURL) { }

        public string GetEntry (string dataname)
        {
            if (m_settingsData.ContainsKey(dataname))
            {
                return m_settingsData[dataname];
            }
            return "";
        }

        public void PutEntry (string dataname, string data)
        {
            m_settingsData[dataname] = data;
        }

        public int GetIntEntry (string dataname)
        {
            int value = 0;
            string valueStr = GetEntry (dataname);
            if (valueStr.Length > 0)
            {
                value = int.Parse(valueStr);
            }
            return value;
        }

        public void PutIntEntry (string dataname, int value)
        {
            PutEntry(dataname, value.ToString());
        }

        public bool GetBoolEntry (string dataname)
        {
            return GetIntEntry (dataname) != 0;
        }

        public void PutBoolEntry (string dataname, bool value)
        {
            PutIntEntry(dataname, value ? 1 : 0);
        }

        // --------------- these don't seem to belong here but where else should they be? -------------------
        protected void SetSharedText(string id, EntryInfo entry, TanWebBrowser wb)
        {
            wb.SetCheckBool("shared_" + id, !entry.IsCustom(this, id));
            wb.SetText(id, entry.GetEntry(this, id));
        }

        protected void SetSharedCheck(string id, EntryInfo entry, TanWebBrowser wb)
        {
            wb.SetCheckBool("shared_" + id, !entry.IsCustom(this, id));
            wb.SetCheck(id, entry.GetEntry(this, id));
        }

        protected void GetSharedText(string id, EntryInfo entry, TanWebBrowser wb)
        {
            bool bShared = wb.GetCheckBool("shared_" + id);
            if (bShared)
            {
                entry.MarkAsShared(this, id);
            }
            else
            {
                entry.MarkAsCustom(this, id);
            }
            entry.PutEntry(this, id, wb.GetText(id), false);
        }

        protected void GetSharedCheck(string id, EntryInfo entry, TanWebBrowser wb)
        {
            bool bShared = wb.GetCheckBool("shared_" + id);
            if (bShared)
            {
                entry.MarkAsShared(this, id);
            }
            else
            {
                entry.MarkAsCustom(this, id);
            }
            entry.PutEntry(this, id, wb.GetCheck(id), false);
        }

        protected void GetSetSharedText(string id, EntryInfo entry, TanWebBrowser wb, bool bGetFromForm)
        {
            if (bGetFromForm)
            {
                GetSharedText(id, entry, wb);
            }
            else
            {
                SetSharedText(id, entry, wb);
            }
        }

        protected void GetSetSharedCheck(string id, EntryInfo entry, TanWebBrowser wb, bool bGetFromForm)
        {
            if (bGetFromForm)
            {
                GetSharedCheck(id, entry, wb);
            }
            else
            {
                SetSharedCheck(id, entry, wb);
            }
        }

        protected void GetSetCustomText(string label, EntryInfo entry, TanWebBrowser wb, bool bGetFromForm)
        {
            if (bGetFromForm)
            {
                // get from the form and put in C# struct
                entry.PutEntry(this, label ,wb.GetText(label), true);
            }
            else
            {
                // get from C# struct and put in form
                wb.SetText(label, entry.GetEntry(this, label));
            }
        }

        protected void GetSetCustomCheck(string label, EntryInfo entry, TanWebBrowser wb, bool bGetFromForm)
        {
            if (bGetFromForm)
            {
                // get from the form and put in C# struct
                entry.PutEntry(this, label, wb.GetCheck(label), true);
            }
            else
            {
                // get from C# struct and put in form
                wb.SetCheck(label, entry.GetEntry(this, label));
            }
        }

        protected void GetSetCustomSelect(string label, EntryInfo entry, TanWebBrowser wb, bool bGetFromForm)
        {
            if (bGetFromForm)
            {
                // get from the form and put in C# struct
                entry.PutEntry(this, label, wb.GetSelect(label), true);
            }
            else
            {
                // get from C# struct and put in form
                wb.SetSelect(label, entry.GetEntry(this, label));
            }
        }

        protected void SetADefault(EntryInfo entry, string label)
        {
            if (entry.GetEntry(this, label).Length == 0)
            {
                entry.PutEntryNoMod(this, label, this.GetEntry(label), true);
            }
        }

        public void GetMorePosts()
        {
            privGetMorePosts(); // add 20 posts
        }

        public void UpdatePosts()
        {
            m_oldPostsInfo.Clear();
            m_usersInfo.Clear();
            GetMorePosts();
        }

        public TanDictionary GetUserInfo(string userid)
        {
            if (m_usersInfo.ContainsKey(userid))
            {
                return m_usersInfo[userid];
            }
            return null;
        }

        public void AddUserInfo(string userid, string name)
        {
            TanDictionary tan = new TanDictionary();

            tan.PutEntry("name", name);
            m_usersInfo[userid] = tan;
        }

        // get 20 more posts than we currently have
        protected virtual void privGetMorePosts()
        {
            MessageBox.Show(Localize.G.GetText("PostsNoPosts"), Localize.G.GetText("PostsSorry"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected int GetNumOldPosts()
        {
            return m_oldPostsInfo.Count;
        }

        protected void ClearOldPosts()
        {
            m_oldPostsInfo.Clear();
        }

        protected TanDictionary AddOldPost(string title, string userid, DateTime date, string postid)
        {
            TanDictionary tan = new TanDictionary();

            m_oldPostsInfo.Add(tan);

            tan.PutEntry("title", title);
            tan.PutEntry("userid", userid);
            tan.PutEntry("postid", postid);
            tan.PutEntry("date", Util.StringFromDate(date));

            return tan;
        }

        public void OpOnPosts(ArrayListOperator alo)
        {
            foreach (Object ob in m_oldPostsInfo)
            {
                if (!alo.operation(ob))
                {
                    break;
                }
            }
        }

        public EntryInfo GetPost (int index)
        {
            TanDictionary post = m_oldPostsInfo[index];
            EntryInfo entry = new EntryInfo();

            PostedEntryInfo pei = new PostedEntryInfo();

            pei.postedID = post.GetEntry("postid");

            privGetPost(entry, ref pei);

            entry.PutEntry (this, EID_POSTED_ID , pei.postedID, true);
            entry.PutEntry (this, EID_POSTED_URL, pei.postedURL, true);

            entry.PutIntEntry("postCount", 1);
            entry.PutIntEntry(this, "previouslyPostedCount",    1, true);
            entry.PutIntEntry(this, "previouslyPublishedCount", 1, true);

            entry.ClearModified();

            return entry;
        }

        protected virtual void privGetPost (EntryInfo entry, ref PostedEntryInfo pei)
        {
            throw new Exception(Localize.G.GetText("ErrorCantGetPosts"));
        }
    }

    public abstract class BlogPhotoServiceCreator
    {
        public abstract BlogPhotoService GetNewService();
    }
}
