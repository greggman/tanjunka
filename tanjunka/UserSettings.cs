#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace Tanjunka
{
    // all the settings for one user (maybe someday we'll support more than one)
    public abstract class ArrayListOperator
    {
        public abstract bool operation (Object ob);
    }

    public class GenericSerializationInfo
    {
       SerializationInfo m_SerializationInfo;
       public GenericSerializationInfo(SerializationInfo info)
       {
          m_SerializationInfo = info;
       }
       public void AddValue<T>(string name,T value)
       {
          m_SerializationInfo.AddValue(name,value,value.GetType());
       }
       public T GetValue<T>(string name)
       {
          object obj = m_SerializationInfo.GetValue(name,typeof(T));
          return (T)obj;
       }
    }

    [Serializable]
    public class PostSet : ISerializable
    {
        bool             m_bOn;
        string           m_name;
        BlogPhotoService m_blogService;
        BlogPhotoService m_photoService;

        public bool IsOn() { return m_bOn; }
        public void SetOn(bool bOn) { m_bOn = bOn; }

        public string GetName () { return m_name; }
        public void SetName (string name) { m_name = name; }

        public override string ToString()
        {
            return GetName();
        }

        public BlogPhotoService GetBlogService() { return m_blogService; }
        public void SetBlogService(BlogPhotoService bps) { m_blogService = bps; }

        public BlogPhotoService GetPhotoService() { return m_photoService; }
        public void SetPhotoService(BlogPhotoService bps) { m_photoService = bps; }

        public PostSet()
        {
            m_bOn = true;
            m_name = "My New Postset";
            m_blogService = null;
            m_photoService = null;
        }

        public string GetBlogServiceName()
        {
            if (m_blogService != null)
            {
                return m_blogService.GetName();
            }
            return "**NA**";
        }

        public string GetPhotoServiceName()
        {
            if (m_photoService != null)
            {
                return m_photoService.GetName();
            }
            return "**NA**";
        }

        // deserialize constructor
        protected PostSet(SerializationInfo info, StreamingContext context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            m_name         = genericInfo.GetValue<string>("name");
            m_bOn          = genericInfo.GetValue<bool>("on");
            m_blogService  = genericInfo.GetValue<BlogPhotoService>("blogService");
            bool bHavePhotoService = genericInfo.GetValue<bool>("havePhotoService");
            if (bHavePhotoService)
            {
                m_photoService = genericInfo.GetValue<BlogPhotoService>("photoService");
            }
            else
            {
                m_photoService = null;
            }
        }

        public void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("name", m_name); //using type inference
            genericInfo.AddValue("on", m_bOn); //using type inference
            genericInfo.AddValue("blogService", m_blogService); //using type inference
            bool bHavePhotoService = (m_photoService != null);
            genericInfo.AddValue("havePhotoService", bHavePhotoService);
            if (bHavePhotoService)
            {
                genericInfo.AddValue("photoService", m_photoService); //using type inference
            }
        }
    }

    [Serializable]
    public class UserSettings : ISerializable
    {
        public static string userSettingsFilename
            {
                get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Greggman\\Tanjunka\\usersettings.bin"); }
            }

        static UInt32 version = 4; // used for dataversion because SerializationInfo is FUCKED!
        static UInt32 loadVersion = 1;
        static UserSettings m_userSettings;
        private Dictionary<string, string> m_settingsData;
        private List<PostSet> postSets; // all defined post sets
        private List<BlogPhotoService> blogSettings; // add defined blog settings

        public UserSettings()
        {
            m_settingsData = new Dictionary<string, string>();
            postSets = new List<PostSet>();
            blogSettings = new List<BlogPhotoService>();

            InitDefaults();
        }

        public static UserSettings G { get { return m_userSettings; } }
        public static UInt32 GetLoadVersion () { return loadVersion; }
        public int GetNumBlogSettings() { return blogSettings.Count; }
        public int GetNumPostSets() { return postSets.Count; }

        public int GetNumOnPostSets()
        {
            int count = 0;
            foreach (PostSet ps in postSets)
            {
                if (ps.IsOn())
                {
                    ++count;
                }
            }
            return count;
        }

        private void SetToDefault(string id, string value)
        {
            if (GetEntry(id).Length == 0)
            {
                PutEntry (id, value);
            }
        }

        public void InitDefaults()
        {
            SetToDefault ("prefs_maxwidth",     "400");
            SetToDefault ("prefs_maxheight",    "400");
            SetToDefault ("prefs_copylocally",  "1");
            SetToDefault ("prefs_showhtml",     "0");
            SetToDefault ("prefs_autogennames", "1");
            SetToDefault ("proxy_useproxy",     "0");
            SetToDefault ("proxy_address",      "server:port");
            SetToDefault ("proxy_useauth",      "0");
            SetToDefault ("proxy_username",     "");
            SetToDefault ("proxy_password",     "");
            SetToDefault ("prefs_showlog",      "0");
            SetToDefault ("prefs_showdebugmenu","0");
            SetToDefault ("prefs_checkfornew",  "1");
        }

        // deserialize constructor
        protected UserSettings(SerializationInfo info, StreamingContext context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            blogSettings = genericInfo.GetValue<List<BlogPhotoService>>("blogSettings");
            postSets = genericInfo.GetValue<List<PostSet>>("postSets");
            m_settingsData = genericInfo.GetValue<Dictionary<string,string>>("m_settingsData");

            // !!!! we can't call this here it fucks up deserialization
            // InitDefaults();
        }

        public void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("blogSettings", blogSettings); //using type inference
            genericInfo.AddValue("postSets", postSets); //using type inference
            genericInfo.AddValue("m_settingsData", m_settingsData); //using type inference

        }

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
            string v = GetEntry(dataname);
            if (v.Length > 0)
            {
                value = int.Parse(v);
            }
            return value;
        }

        public void PutIntEntry (string dataname, int data)
        {
            PutEntry(dataname, data.ToString());
        }

        public bool GetBoolEntry (string dataname)
        {
            return GetIntEntry(dataname) != 0;
        }

        public void PutBoolEntry (string dataname, bool data)
        {
            PutIntEntry(dataname, data ? 1 : 0);
        }

        public void AddBlogSettings (BlogPhotoService bps)
        {
            blogSettings.Add(bps);
        }

        public void AddPostSet (PostSet ps)
        {
            postSets.Add(ps);
        }

        public void OpOnPostSets(ArrayListOperator alo)
        {
            foreach (Object ob in postSets)
            {
                if (!alo.operation(ob))
                {
                    break;
                }
            }
        }

        public void OpOnOnlyOnPostSets(ArrayListOperator alo)
        {
            foreach (Object ob in postSets)
            {
                PostSet ps = (PostSet)ob;
                if (ps.IsOn())
                {
                    if (!alo.operation(ob))
                    {
                        break;
                    }
                }
            }
        }

        public bool BlogSettingInUse(BlogPhotoService bps)
        {
            foreach (PostSet ps in postSets)
            {
                if (bps == ps.GetBlogService() || bps == ps.GetPhotoService())
                {
                    return true;
                }
            }
            return false;
        }

        public PostSet GetPostSetByIndex (int index)
        {
            return postSets[index];
        }

        public void DeletePostSetByIndex (int index)
        {
            postSets.RemoveAt(index);
        }

        public void DeleteBlogSettingByIndex (int index)
        {
            if (!BlogSettingInUse(GetBlogSettingsByIndex(index)))
            {
                blogSettings.RemoveAt(index);
            }
        }

        public void OpOnBlogSettings(ArrayListOperator alo)
        {
            foreach (Object ob in blogSettings)
            {
                if (!alo.operation(ob))
                {
                    break;
                }
            }
        }

        public BlogPhotoService GetBlogSettingsByIndex (int index)
        {
            return blogSettings[index];
        }

        public static void Load()
        {
            if (File.Exists(UserSettings.userSettingsFilename))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(userSettingsFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    loadVersion = (UInt32)formatter.Deserialize(stream);
                    //m_userSettings = formatter.Deserialize(stream) as UserSettings;
                    m_userSettings = formatter.Deserialize(stream) as UserSettings;
                }
                catch (Exception ex)
                {
                    //Log.Printn("EX: " + ex.Message);
                    MessageBox.Show(Localize.G.GetText("ErrorBadSettings") + "\n\n" + ex.Message);
                    m_userSettings = new UserSettings();
                }
                stream.Close();
            }
            else
            {
                m_userSettings = new UserSettings();
            }
            UserSettings.G.InitDefaults();
        }

        public static void Save()
        {
            string oldName = userSettingsFilename + ".old";
            string tempName = userSettingsFilename + ".temp";

            if (!Directory.Exists(Path.GetDirectoryName(userSettingsFilename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(userSettingsFilename));
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(tempName, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, version);
            formatter.Serialize(stream, m_userSettings);
            stream.Close();

            // delete old backup
            if (File.Exists(oldName))
            {
                File.Delete(oldName);
            }
            // rename previous settings to old
            if (File.Exists(userSettingsFilename))
            {
                File.Move(userSettingsFilename, oldName);
            }
            // rename temp to settings
            File.Move(tempName, userSettingsFilename);
        }
    }
}
