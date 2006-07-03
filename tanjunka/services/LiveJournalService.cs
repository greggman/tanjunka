#region Using directives

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

using CookComputing.XmlRpc;

#endregion

namespace Tanjunka
{
    [Serializable]
    public class LiveJournalService : BloggerAPIService
    {
        private BloggerAPIProxy bapi;

        public LiveJournalService()
        {
            bapi = new BloggerAPIProxy();
            bapi.KeepAlive = false;

            PutEntry("xmlrpcURL", "http://www.livejournal.com/interface/blogger/");
        }

        public override bool CanEncode() { return false; }

        // deserialize constructor
        protected LiveJournalService(SerializationInfo info, StreamingContext context) : base (info, context)
        {
            bapi = new BloggerAPIProxy();
            bapi.KeepAlive = false;
        }

        public override void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);
        }

        public override string GetServiceTypeName() { return "LiveJournal"; }

        protected override bool privatePostEntry(EntryInfo entry, PostInfo postInfo, ref PostedEntryInfo pei)
        {
            entry.PutIntEntry(this, "publish", 1, true);
            string moodid = entry.GetEntry(this, "prop_current_moodid");
            string mood   = entry.GetEntry(this, "prop_current_mood");
            string music  = entry.GetEntry(this, "prop_current_music");

            postInfo.bPublish = true;
            postInfo.PutEntry("postbody",
                ((moodid.Length > 0 || mood.Length > 0) ? ("lj-mood: " + moodid + " " + mood + "\n") : "") +
                ((music.Length > 0) ? ("lj-music: " + music + "\n") : "") +
                "<title>" + entry.GetEntry("posttitle") + "</title>" +
                postInfo.GetEntry("postbody"));

            return base.privatePostEntry(entry, postInfo, ref pei);
        }

        public override bool OpenSettingsForm(IWin32Window win)
        {
            bool result = false;
            LiveJournalForm form = new LiveJournalForm(this);
            if (form.ShowDialog() == DialogResult.OK)
            {
                result = true;
            }
            form.Dispose();

            return result;
        }
        public override bool DisplayPostOptionsForm() { return false; }
        public override string GetOptionForm() { return "postoptions-livejournal.html"; }

        private void GetSetAll(EntryInfo entry, TanWebBrowser wb, bool bGetFromForm)
        {
            GetSetCustomSelect("prop_current_moodid", entry, wb, bGetFromForm);
            GetSetCustomText("prop_current_mood", entry, wb, bGetFromForm);
            GetSetCustomText("prop_current_music", entry, wb, bGetFromForm);
        }

        public override void updateEntryToOptions(PostSet ps, EntryInfo entry, TanWebBrowser wb)
        {
            entry.PutEntryNoMod(this, "publish", "1", true);
            base.updateEntryToOptions(ps, entry, wb);
            GetSetAll (entry, wb, false);
        }

        public override void updateOptionsToEntry(PostSet ps, EntryInfo entry, TanWebBrowser wb)
        {
            base.updateOptionsToEntry(ps, entry, wb);
            GetSetAll (entry, wb, true);
        }


        protected override string GetBloggerTitleFromBody(string body)
        {
            Regex r = Util.MakeRegex("<title\\s*>(?<1>.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Match m = r.Match(body);
            if (m.Success)
            {
                return Util.StripHTML(m.Groups[1].ToString());
            }
            else
            {
                return base.GetBloggerTitleFromBody(body);
            }
        }

        // I had to do this because livejournal FUCKED up and
        // changed the capitalization of 2 fields
        // userid, postid become userID, postId. XML is case sensitive!
        protected override void privGetMorePosts()
        {
            try
            {
                bapi.Url = GetEntry("xmlrpcURL");

                livejournal_RecentPost[] titles = bapi.livejournal_getRecentPosts(
                    "appkey",
                    GetEntry("blogid"),
                    GetEntry("username"),
                    GetEntry("password"),
                    GetNumOldPosts() + 20
                    );

                ClearOldPosts();

                foreach (livejournal_RecentPost title in titles)
                {
                    TanDictionary tan = AddOldPost(
                        GetBloggerTitleFromBody(title.content),
                        "", //title.userID,
                        title.dateCreated,
                        title.postId);

                    tan.PutEntry("blogger_content", title.content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Localize.G.GetText("LabelUhOh"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override string BloggerifyContent(EntryInfo entry, string content)
        {
            // extract title, mood and music
            {
                Regex r = Util.MakeRegex("<title\\s*>(?<1>.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Match m = r.Match(content);
                if (m.Success)
                {
                    entry.PutEntry(this, "posttitle", m.Groups[1].ToString(), false);
                    content = r.Replace(content, "");
                }
            }

            {
                Regex r = Util.MakeRegex("lj-mood: (?<1>.*?)\\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Match m = r.Match(content);
                if (m.Success)
                {
                    entry.PutEntry(this, "prop_current_mood", m.Groups[1].ToString(), true);
                    content = r.Replace(content, "");
                }
            }

            {
                Regex r = Util.MakeRegex("lj-music: (?<1>.*?)\\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Match m = r.Match(content);
                if (m.Success)
                {
                    entry.PutEntry(this, "prop_current_music", m.Groups[1].ToString(), true);
                    content = r.Replace(content, "");
                }
            }

            return content;

        }

        //--------------------- live journal only -----------

        //--------------- blogger API kludge stuff -------------
        #if false
        protected virtual bool kludge_editPost(string appkey, string postid, string username, string password, string content, bool publish)
        {
            return bapi.blogger_editPost_intResponse(appkey, postid, username, password, content, publish);
        }
        #endif

    }

    public class LiveJournalServiceCreator : BlogPhotoServiceCreator
    {
        public override BlogPhotoService GetNewService()
        {
            return new LiveJournalService();
        }
    }
}

