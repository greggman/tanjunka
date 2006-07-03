#region Using directives

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization;

using FlickrNet;

#endregion

namespace Tanjunka
{
    [Serializable]
    public class FlickrService : BlogPhotoService
    {
        private const string appkey = "8636717c3475d7c19600a9c8a2c0f463";
        private Flickr fapi;

        public FlickrService()
        {
            fapi = new Flickr(appkey);
        }

        // deserialize constructor
        protected FlickrService(SerializationInfo info, StreamingContext context) : base (info, context)
        {
            fapi = new Flickr(appkey);
        }

        public override void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);
        }

        public override string GetServiceTypeName() { return "Flickr"; }

        // give this service a chance to allocate some resources for posting
        public override void Startup()
        {
            fapi.Email = GetEntry("username");
            fapi.SetPassword(GetEntry("password"));
            // famp.Proxy = ??
            //old: fuser = fapi.Authenticate(GetEntry("username"), GetEntry("password"));
        }

        public override void Shutdown() { } // give this service a chance to free resoruces needed for posting

        public override bool CanPostPhoto() { return true; }
        public override bool CanPostEntry() { return false; }

        public override bool HaveRoomFor(long numPhotos, long totalBytes)
        {
            //todo
            //return (totalBytes <= (fuser.Transfer.Limit - fuser.Transfer.Used))
            return true;
        }

        protected override bool privatePostPhoto(EntryInfo entry, TJPagePicture tjpic, ref PostedPhotoInfo ppi)
        {
            Log.Printn("uploading : (" + Path.GetFileName(tjpic.GetOriginalPath()) + ")");
            Log.Printn("upload to : (Flickr)");

            string filename = tjpic.GetTempPath();
            bool bUseTempPic = false;

            // convert non jpgs to jpgs
            string ext = Path.GetExtension(filename).ToLower();
            if (ext.CompareTo(".jpg") != 0 && ext.CompareTo(".jpeg") != 0)
            {
                filename = TJPagePicture.MakeJPEG(filename);
                bUseTempPic = true;
            }

            // UploadPicture(string filename, string title, string description, string tags, bool isPublic, bool isFamily, bool isFriend);
            string photoID = fapi.UploadPicture(
                filename,
                Path.GetFileName(tjpic.GetOriginalPath()),
                "", // description
                "", // tags
                true, // isPublic
                false, // isFamily
                false  // isFriend
                );

            Log.Printn("photo id  : " + photoID);

            PhotoInfo pinfo = fapi.PhotosGetInfo(photoID);

            ppi.postedURL  = "http://photos" + pinfo.server + ".flickr.com/" + photoID + "_" + pinfo.secret + "_o.jpg";
            ppi.postedSize.Width = tjpic.GetNewWidth();
            ppi.postedSize.Height = tjpic.GetNewHeight();
            // http://www.flickr.com/photos/51035661423@N01/769164/
            ppi.largerURL  = "http://www.flickr.com/photos/" + pinfo.Owner.UserId + "/" + photoID + "/";

            Log.Printn("photo URL : " + ppi.postedURL);

            if (bUseTempPic)
            {
                File.Delete(filename);
            }

            return true;
        }

        public override bool OpenSettingsForm(IWin32Window win)
        {
            bool result = false;
            FlickrForm form = new FlickrForm(this);
            if (form.ShowDialog() == DialogResult.OK)
            {
                result = true;
            }
            form.Dispose();

            return result;
        }
        public override bool DisplayPostOptionsForm() { return false; }

        class ImgFixer
        {
            EntryInfo m_entry;
            BlogPhotoService m_ps;

            public ImgFixer(EntryInfo entry, BlogPhotoService ps)
            {
                m_entry = entry;
                m_ps = ps;
            }

            string AddFunkyTag (Match m)
            {
                return m.Groups[1].ToString() + "tanfunky=\"true\" " + m.Groups[2].ToString() + m.Groups[3].ToString();
            }

            string AddRemoveFunkyTag1(Match m)
            {
                Regex r = Util.MakeRegex("tanfunky\\s*=\\s*(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*)", RegexOptions.IgnoreCase|RegexOptions.Compiled);
                string middle = r.Replace(m.Groups[3].ToString(), "");

                return m.Groups[1].ToString() +
                       m.Groups[2].ToString() +
                       middle +
                       m.Groups[4].ToString() +
                       m.Groups[5].ToString();
            }

            string AddFlickrTag(Match m)
            {
                // make sure it's got a tanfunky tag
                string middle = m.Groups[2].ToString();
                Regex r = Util.MakeRegex("tanfunky\\s*=\\s*(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*)", RegexOptions.IgnoreCase|RegexOptions.Compiled);
                Match m2 = r.Match(middle);
                if (m2.Success)
                {
                    middle = r.Replace(middle, "");
                    string largeURL = "";

                    Regex r2 = Util.MakeRegex("src\\s*=\\s*(?<1>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*))", RegexOptions.IgnoreCase|RegexOptions.Compiled);
                    Match srcMatch = r2.Match(middle);
                    if (srcMatch.Groups.Count > 1)
                    {
                        string src = Util.RemoveQuotes(srcMatch.Groups[1].ToString());

                        if (m_entry.PictureTempExists(src))
                        {
                            TJPagePicture tjpic = m_entry.GetTempPicture(src);
                            largeURL = tjpic.GetPhotoEntry(m_ps, PSID_PIC_LARGER_URL);
                        }
                    }

                    if (largeURL.Length > 0)
                    {
                        return "<a href=\"" + largeURL + "\">" + m.Groups[1].ToString() + middle + m.Groups[3].ToString() + "</a>";
                    }
                }

                return m.Groups[1].ToString() + middle + m.Groups[3].ToString();
            }

            public string FixImages(string str)
            {
                // add bogus tag to all <img> tags
                {
                    Regex r = Util.MakeRegex("(?<1><img\\s+)(?<2>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[^>\"'/])*)(?<3>/*>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    str = r.Replace(str, new MatchEvaluator(AddFunkyTag));
                }

                // remove bogus tag from <a><img bogus=""></a>
                {
                    Regex r = Util.MakeRegex("(?<1><a\\s+(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[^>\"'/])*/*>\\s*)(?<2><img\\s+)(?<3>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[^>\"'/])*)(?<4>/*>)(?<5></a\\s*>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    str = r.Replace(str, new MatchEvaluator(AddRemoveFunkyTag1));
                }

                // convert <img bogus=""> to <a href="flickr.com"><img></a>
                {
                    Regex r = Util.MakeRegex("(?<1><img\\s+)(?<2>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[^>\"'/])*)(?<3>/*>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    str = r.Replace(str, new MatchEvaluator(AddFlickrTag));
                }
                return str;
            }
        }

        public override string FilterImageTags (EntryInfo entry, string str)
        {
            ImgFixer ifix = new ImgFixer(entry, this);

            return ifix.FixImages(str);
        }

    }
    public class FlickrServiceCreator : BlogPhotoServiceCreator
    {
        public override BlogPhotoService GetNewService()
        {
            return new FlickrService();
        }
    }
}

