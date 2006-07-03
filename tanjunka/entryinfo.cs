#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;

#endregion

// these comments are also to see if perforce notices
// more test and more and more and more

namespace Tanjunka
{
    // this is all the info for a single post
    // all the data is stored in a Hashtable
    // when requesting the data each service
    // passes in it's service name and whether
    // that option can be shared.
    //
    // If it can then that object had have multiple entires
    // one for each service and one global.
    //
    // Think of of it like this:
    //
    //   * Service "gman mt" asks for "title"
    //
    //   * EntryInfo checks for "gman-mt/title/custom"
    //     If it exists
    //          check for "gman-mt/title"
    //          if that does not exist
    //              copy "title" to "gman-mt/title"
    //          return "gman-mt/title"
    //     else
    //          return "title"
    //
    //
    //
    [Serializable]
    public class PostServiceInfo : ISerializable
    {
        Dictionary<string,bool>     m_customFlags;    // do I really need this?  the idea was it could keep it's old value even if it was set as shared
        Dictionary<string,string>   m_postData;
        string    m_servicename;    // easier to debug if this is here

        public PostServiceInfo (string servicename)
        {
            m_servicename = servicename;
            m_customFlags = new Dictionary<string, bool>();
            m_postData    = new Dictionary<string, string>();
        }

        // deserialize constructor
        protected PostServiceInfo(SerializationInfo info, StreamingContext context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            m_customFlags  = genericInfo.GetValue<Dictionary<string,bool>>("m_customFlags");
            m_postData     = genericInfo.GetValue<Dictionary<string,string>>("m_postData");
        }

        public void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("m_customFlags", m_customFlags);
            genericInfo.AddValue("m_postData", m_postData);
        }

        public string GetEntry (string dataname)
        {
            if (m_postData.ContainsKey(dataname))
            {
                return m_postData[dataname];
            }
            return "";
        }

        public void PutEntry (string dataname, string data)
        {
            m_postData[dataname] = data;
        }

        public void MarkAsCustom (string dataname)
        {
            m_customFlags[dataname] = true;
        }

        public void MarkAsShared (string dataname)
        {
            if (m_customFlags.ContainsKey(dataname))
            {
                m_customFlags.Remove(dataname);
            }
        }

        public bool IsCustom (string dataname)
        {
            return m_customFlags.ContainsKey(dataname);
        }
    }

    [Serializable]
    public class EntryInfo : ISerializable
    {
        private static int m_version = 2;   // used for dataversion because SerializationInfo is FUCKED!
        private static int m_loadVersion = 0;   // used for dataversion because SerializationInfo is FUCKED!
        private Dictionary<string, PostServiceInfo> m_postServiceInfos;
        private PostServiceInfo m_globalInfo;
//        private bool m_bRemake;
        private bool m_bModified;

        private Dictionary<string, TJPagePicture>  m_picList;       // list by original name
        private Dictionary<string, TJPagePicture>  m_tempPicList;   // list by temp name
        private List<TJPagePicture> m_htmlImageList;

        public EntryInfo()
        {
            m_postServiceInfos = new Dictionary<string, PostServiceInfo>();
            m_globalInfo       = new PostServiceInfo("__global_service_info__");
            m_picList          = new Dictionary<string, TJPagePicture>();
            m_tempPicList      = new Dictionary<string, TJPagePicture>();
            m_htmlImageList    = new List<TJPagePicture>();
//            m_bRemake          = false;
            m_bModified        = false;
        }

        public static int GetLoadVersion() { return m_loadVersion; }
        public static void SetLoadVersion(int v) { m_loadVersion = v; }
        public static int GetVersion() { return m_version; }

        // deserialize constructor
        protected EntryInfo(SerializationInfo info, StreamingContext context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            m_postServiceInfos = genericInfo.GetValue<Dictionary<string,PostServiceInfo>>("m_postServiceInfos");
            m_globalInfo       = genericInfo.GetValue<PostServiceInfo>("m_globalInfo");
            m_picList          = genericInfo.GetValue<Dictionary<string, TJPagePicture>>("m_picList");
            m_tempPicList      = genericInfo.GetValue<Dictionary<string, TJPagePicture>>("m_tempPicList");
            m_htmlImageList    = new List<TJPagePicture>();
//            m_bRemake          = true;
            m_bModified        = false;
        }

        public void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("m_postServiceInfos", m_postServiceInfos);
            genericInfo.AddValue("m_globalInfo", m_globalInfo);
            genericInfo.AddValue("m_picList", m_picList);
            genericInfo.AddValue("m_tempPicList", m_tempPicList);

            m_bModified = false;
        }

        public void Dispose()
        {
            cleanupTempFiles();
        }

        public bool IsModified () { return m_bModified; }
        public void ClearModified() { m_bModified = false; }

        public int GetIntEntry (BlogPhotoService bps, string dataname)
        {
            int value = 0;
            string valueStr = GetEntry (bps, dataname);
            if (valueStr.Length > 0)
            {
                value = int.Parse(valueStr);
            }
            return value;
        }

        public int GetIntEntry (string dataname)
        {
            return GetIntEntry (null, dataname);
        }

        public void PutIntEntry (BlogPhotoService bps, string dataname, int value, bool bForceCustom)
        {
            PutEntry(bps, dataname, value.ToString(), bForceCustom);
        }

        public void PutIntEntry (string dataname, int value)
        {
            PutIntEntry (null, dataname, value, false);
        }

        public bool GetBoolEntry (BlogPhotoService bps, string dataname)
        {
            return GetIntEntry (bps, dataname) != 0;
        }

        public bool GetBoolEntry (string dataname)
        {
            return GetBoolEntry (null, dataname);
        }

        public void PutBoolEntry (BlogPhotoService bps, string dataname, bool value, bool bForceCustom)
        {
            PutIntEntry(bps, dataname, value ? 1 : 0, bForceCustom);
        }

        public void PutBoolEntry (string dataname, bool value)
        {
            PutBoolEntry (null, dataname, value, false);
        }

        public string GetEntry (BlogPhotoService bps, string dataname)
        {
            // get the one for the service
            PostServiceInfo psi = GetPostServiceInfo(bps);
            if (psi != null)
            {
                // if it exists then if it's marked as custom get it
                if (psi.IsCustom(dataname))
                {
                    return psi.GetEntry (dataname);
                }
            }
            // return the global one
            return m_globalInfo.GetEntry (dataname);
        }

        public bool IsCustom (BlogPhotoService bps, string dataname)
        {
            // get the one for the service
            PostServiceInfo psi = GetPostServiceInfo(bps);
            if (psi != null)
            {
                // if it exists then if it's marked as custom get it
                return psi.IsCustom(dataname);
            }
            return false;
        }

        public string GetEntry (string dataname)
        {
            return GetEntry (null, dataname);
        }

        public void PutEntry (BlogPhotoService bps, string dataname, string data, bool bForceCustom)
        {
            privPutEntry(bps, dataname, data, bForceCustom, false);
        }

        public void PutEntryNoMod (BlogPhotoService bps, string dataname, string data, bool bForceCustom)
        {
            privPutEntry(bps, dataname, data, bForceCustom, true);
        }

        private void privPutEntry (BlogPhotoService bps, string dataname, string data, bool bForceCustom, bool bNoMod)
        {
            PostServiceInfo psi;

            // if forcecustom just add to service
            if (bForceCustom)
            {
                psi = GetOrCreatePostServiceInfo(bps);
                psi.MarkAsCustom(dataname);
                if (data.CompareTo(psi.GetEntry(dataname)) != 0)
                {
                    if (!bNoMod)
                    {
                        //Log.Printn("--mod force custom (" + dataname + ")--");
                        //Log.Printn("old:(" + psi.GetEntry(dataname) + ")");
                        //Log.Printn("new:(" + data + ")");
                        m_bModified = true;
                    }
                    psi.PutEntry(dataname, data);
                }
                return;
            }
            else
            {
                // see if service has a custom one
                psi = GetPostServiceInfo(bps);
                if (psi != null)
                {
                    // yes? set it
                    if (psi.IsCustom(dataname))
                    {
                        if (data.CompareTo(psi.GetEntry(dataname)) != 0)
                        {
                            if (!bNoMod)
                            {
                                //Log.Printn("--mod custom (" + dataname + ")--");
                                //Log.Printn("old:(" + psi.GetEntry(dataname) + ")");
                                //Log.Printn("new:(" + data + ")");
                                m_bModified = true;
                            }
                            psi.PutEntry(dataname, data);
                        }
                        return;
                    }
                }
            }
            // set the global one
            if (data.CompareTo(m_globalInfo.GetEntry(dataname)) != 0)
            {
                if (!bNoMod)
                {
                    //Log.Printn("--mod global (" + dataname + ")--");
                    //Log.Printn("old:(" + m_globalInfo.GetEntry(dataname) + ")");
                    //Log.Printn("new:(" + data + ")");
                    m_bModified = true;
                }
                m_globalInfo.PutEntry(dataname, data);
            }
        }

        public void PutEntry (string dataname, string data)
        {
            PutEntry (null, dataname, data, false);
        }

        public void PutEntryNoMod (string dataname, string data)
        {
            PutEntryNoMod (null, dataname, data, false);
        }

        private void privMarkAsCustom (BlogPhotoService bps, string dataname, bool bNoMod)
        {
            PostServiceInfo psi = GetOrCreatePostServiceInfo(bps);
            if (!psi.IsCustom(dataname))
            {
                if (!bNoMod)
                {
                    //Log.Printn("--mark as custom--");
                    //Log.Printn("(" + dataname + ")");
                    m_bModified = true;
                }
                psi.MarkAsCustom(dataname);
            }
        }

        public void MarkAsCustom (BlogPhotoService bps, string dataname)
        {
            privMarkAsCustom (bps, dataname, false);
        }
        public void MarkAsCustomNoMod (BlogPhotoService bps, string dataname)
        {
            privMarkAsCustom (bps, dataname, true);
        }

        public void MarkAsShared(BlogPhotoService bps, string dataname)
        {
            PostServiceInfo psi = GetPostServiceInfo(bps);
            if (psi != null)
            {
                if (psi.IsCustom(dataname))
                {
                    //Log.Printn("--mark as shared--");
                    //Log.Printn("(" + dataname + ")");
                    m_bModified = true;
                    psi.MarkAsShared(dataname);
                }
            }
        }

        // ----------------pic stuff---------------------------------------

        public void ClearHtmlImageList()
        {
            m_htmlImageList.Clear();
        }

        public void AddImageToHtmlImageList(TJPagePicture tjpic)
        {
            m_htmlImageList.Add(tjpic);
        }

        public int GetNumHtmlImages()
        {
            return m_htmlImageList.Count;
        }

        public void OpOnHtmlImages(ArrayListOperator alo)
        {
            foreach (Object ob in m_htmlImageList)
            {
                if (!alo.operation(ob))
                {
                    break;
                }
            }
        }

        public void FixupNewVsOldTempPaths()
        {
            // remove ALL from the tempPicList dict
            m_tempPicList.Clear();

            // walk through the picList dist and add them back
            Dictionary<string, TJPagePicture>.Enumerator de = m_picList.GetEnumerator();
            while (de.MoveNext())
            {
                TJPagePicture tjpic = de.Current.Value;

                tjpic.resetTempPath();
                m_tempPicList[Path.GetFileName(tjpic.GetTempPath())] = tjpic;
            }
        }

        public bool PictureExists(string name)
        {
            return m_picList.ContainsKey(Path.GetFileName(name));
        }

        public bool PictureTempExists(string name)
        {
            return m_tempPicList.ContainsKey(Path.GetFileName(name));
        }

        // returns a picture on the list
        // it must exist or you'll get an exception
        // (see PictureExists)
        public TJPagePicture GetPicture(string name)
        {
            Object ob = m_picList[Path.GetFileName(name)];
            return (TJPagePicture)ob;
        }

        // returns a picture on the temp list
        // it must exist or you'll get an exception
        // (see PictureTempExists)
        public TJPagePicture GetTempPicture(string name)
        {
            Object ob = m_tempPicList[Path.GetFileName(name)];
            return (TJPagePicture)ob;
        }

        // check our list of images and see if this
        // image is already in the list,
        // if not, add it

        // load it, rescale it, save it to a temp file
        // return the <img> tag we need to display it
        // probably needs an "onresize" tag
        public string AddPicture(string filename)
        {
            TJPagePicture tpic = GetNewPicture(filename);
            {
                string html = "<img" +
                    " src=\"" + tpic.GetTempPath() + "\"" +
                    " width=\"" + tpic.GetNewWidth() + "\"" +
                    " height=\"" + tpic.GetNewHeight() + "\"" +
//                    " tanorigwidth=\"" + tpic.GetOrigWidth() + "\"" +
//                    " tanorigheight=\"" + tpic.GetOrigHeight() + "\"" +
                    " />";

                return html;
            }
        }

        public TJPagePicture GetNewPicture(string filename)
        {
            if (!PictureExists(filename))
            {
                TJPagePicture tpic = new TJPagePicture(filename);

                m_picList[Path.GetFileName(filename)] = tpic;

                tpic.resizeTempFileToFit(
                        int.Parse(UserSettings.G.GetEntry("prefs_maxwidth")),
                        int.Parse(UserSettings.G.GetEntry("prefs_maxheight")),
                        false);

                m_tempPicList[Path.GetFileName(tpic.GetTempPath())] = tpic;
            }

            return GetPicture(filename);
        }

        public void resizeTempFile(TJPagePicture tpic, int newWidth, int newHeight)
        {
            // the temp filename might change so remove it from the tempList
            m_tempPicList.Remove(Path.GetFileName(tpic.GetTempPath()));
            // make the new file
            tpic.resizeTempFile(newWidth, newHeight);
            // add it back to the templist
            m_tempPicList[Path.GetFileName(tpic.GetTempPath())] = tpic;
        }

        public void resizeTempFileToFit(TJPagePicture tpic, int newWidth, int newHeight, bool bForce)
        {
            // the temp filename might change so remove it from the tempList
            m_tempPicList.Remove(Path.GetFileName(tpic.GetTempPath()));
            // make the new file
            tpic.resizeTempFileToFit(newWidth, newHeight, bForce);
            // add it back to the templist
            m_tempPicList[Path.GetFileName(tpic.GetTempPath())] = tpic;
        }

        public void ConvertToJPEG(TJPagePicture tpic)
        {
            // the temp filename might change so remove it from the tempList
            m_tempPicList.Remove(Path.GetFileName(tpic.GetTempPath()));
            // make the new file
            MessageBox.Show("convert to jpeg not implemented", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
//           tpic.ConvertToJPEG();
            // add it back to the templist
            m_tempPicList[Path.GetFileName(tpic.GetTempPath())] = tpic;
        }

        public void cleanupTempFiles()
        {
            // todo: make this CLS complient
            Dictionary<string, TJPagePicture>.Enumerator de = m_picList.GetEnumerator();
            while (de.MoveNext())
            {
                de.Current.Value.deleteTempFile();
            }

            #if false
            foreach (DictionaryEntry<string, TJPagePicture>.Entry de in m_picList)
            {
                Object ob = de.Value;
                TJPagePicture tpic = (TJPagePicture)ob;
                tpic.deleteTempFile();
            }
            #endif

            m_picList.Clear();

        }

        // ----------------------------------------------------------------

        private PostServiceInfo CreatePostServiceInfo (BlogPhotoService bps)
        {
            PostServiceInfo psi = new PostServiceInfo(bps.GetID());
            m_postServiceInfos[bps.GetID()] = psi;

            return psi;
        }

        private PostServiceInfo GetPostServiceInfo (BlogPhotoService bps)
        {
            if (bps != null && m_postServiceInfos.ContainsKey(bps.GetID()))
            {
                Object ob = m_postServiceInfos[bps.GetID()];
                return (PostServiceInfo)ob;
            }
            return null;
        }

        private PostServiceInfo GetOrCreatePostServiceInfo (BlogPhotoService bps)
        {
            if (m_postServiceInfos.ContainsKey(bps.GetID()))
            {
                Object ob = m_postServiceInfos[bps.GetID()];
                return (PostServiceInfo)ob;
            }
            return CreatePostServiceInfo (bps);
        }
    }
}

