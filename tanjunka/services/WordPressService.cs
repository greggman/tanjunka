#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Net;
using CookComputing.XmlRpc;

#endregion

namespace Tanjunka
{
    // concrete class to handle the moveabletype service
    [Serializable]
    public class WordPressService : MovabletypeService
    {
        public WordPressService()
        {
        }

        // deserialize constructor
        protected WordPressService(SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);
        }

        public override string GetServiceTypeName() { return "WordPress"; }

        public override bool CanEncode() { return false; }

        public override bool OpenSettingsForm(IWin32Window win)
        {
            bool result = false;
            WordPressForm form = new WordPressForm(this);
            if (form.ShowDialog() == DialogResult.OK)
            {
                result = true;
            }
            form.Dispose();

            return result;
        }

        //--------------- movabletype API kludge stuff -------------
        public override bool kludge_publish(string postid, string username, string password)
        {
            return MAPI.mt_publish_intResponse(postid, username, password) != 0;
        }
    }

    public class WordPressServiceCreator : BlogPhotoServiceCreator
    {
        public override BlogPhotoService GetNewService()
        {
            return new WordPressService();
        }
    }
}

