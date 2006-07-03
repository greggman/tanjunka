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
    class GManForm : ServiceSettingsForm
    {
        public GManForm(BlogPhotoService bps) :
            base(bps, "settings-GManform.html", "GMan Settings")
        {
        }

        public GManForm(BlogPhotoService bps, string formURL, string title) :
            base(bps, formURL, title)
        {
        }

        protected override void privGetSetAll (bool bGetFromForm)
        {
            GetSetText("servicename", bGetFromForm);
            GetSetText("username",    bGetFromForm);
            GetSetText("password",    bGetFromForm);
            GetSetText("xmlrpcURL",   bGetFromForm);
            GetSetText("stylesheet",  bGetFromForm);

            if (bGetFromForm)
            {
            }
            else
            {
            }
            // encoding = select
            // copystylesheet = bool
        }

        protected override void ClickSomething (int id)
        {
            switch (id)
            {
            case 10: // guess info
                break;
            case 11: // test xmlrpc
                break;
            case 12: // update blogid list
                break;
            default:
                base.ClickSomething(id);
                break;
            }
        }
    }
}