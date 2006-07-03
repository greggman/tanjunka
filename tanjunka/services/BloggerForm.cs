#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

#endregion

namespace Tanjunka
{
    // form for editing moveable type service options
    class BloggerForm : BloggerAPIForm
    {
        public BloggerForm(BlogPhotoService bps) :
            base(bps, "settings-bloggerform.html", "Blogger Settings")
        {
        }

        protected override void GetSetExtra (bool bGetFromForm)
        {
        }

        protected override string GetXMLRPCURL()
        {
            return m_bps.GetEntry("xmlrpcURL");
        }
    }
}