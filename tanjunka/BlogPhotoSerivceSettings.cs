#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace Tanjunka
{
    // the abstract interface for a settings for a service
    public abstract class BlogPhotoServiceSettings
    {
        Hashtable           m_settingsData;
        BlogPhotoService    m_serivce;

        public BlogPhotoServiceSettings(BlogPhotoService serivce)
        {
            m_service      = service;
            m_settingsData = new Hashtable();
        }

        public string GetEntry (string dataname)
        {
            if (m_settingsData.ContainsKey(dataname))
            {
                Object ob = m_postData[dataname];
                return (string)ob;
            }
            return "";
        }

        public void PutEntry (string dataname, string data)
        {
            m_settingsData[dataname] = data;
        }


    }
}

