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
    class FlickrForm : ServiceSettingsForm
    {
        // blogname
        // username
        // password

        public FlickrForm(BlogPhotoService bps) :
            base(bps, "settings-flickrform.html", "Flickr Settings")
        {
        }

        protected override void privGetSetAll (bool bGetFromForm)
        {
            GetSetText("username",    bGetFromForm);
            GetSetText("password",    bGetFromForm);
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
            case 13: // update stylesheet
                break;
            //-- not used
            case 14: // browser for bodyform
                break;
            case 15: // browse for moreform
                break;
            }
        }
    }
}