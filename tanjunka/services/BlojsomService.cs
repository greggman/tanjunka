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
    public class BlojsomService : MovabletypeService
    {
        public BlojsomService()
        {
        }

        // deserialize constructor
        protected BlojsomService(SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);
        }

        public override string GetServiceTypeName() { return "Blojsom"; }

        public override bool CanEncode() { return false; }

        public override bool OpenSettingsForm(IWin32Window win)
        {
            bool result = false;
            BlojsomForm form = new BlojsomForm(this);
            if (form.ShowDialog() == DialogResult.OK)
            {
                result = true;
            }
            form.Dispose();

            return result;
        }
    }

    public class BlojsomServiceCreator : BlogPhotoServiceCreator
    {
        public override BlogPhotoService GetNewService()
        {
            return new BlojsomService();
        }
    }
}

