#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Tanjunka
{
    // this is the data used specifically during a post.
    // it is NOT saved, it is temporary
    // for example between the time of the user picking PUBLISH and the time
    // the post is actually sent the <img tags> have to be fixed for each
    // service.  This struct holds that modified data
    #if false
    public class TanStringDict : Dictionary<string,string>
    {
        public string GetEntry (string dataname)
        {
            if (ContainsKey(dataname))
            {
                return this[dataname];
            }
            return "";
        }

        public void PutEntry (string dataname, string data)
        {
            this[dataname] = data;
        }
    }
    #endif

    public class PostInfo
    {
        Dictionary<string,string>   m_postData;
        public bool bPublish;
        public bool bPrevPublished;
        public bool bPrevPosted;

        public PostInfo ()
        {
            m_postData    = new Dictionary<string, string>();
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
}
