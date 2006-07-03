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
    public class SixApartMovabletypeService : MovabletypeService
    {
        public SixApartMovabletypeService()
        {
        }

        // deserialize constructor
        protected SixApartMovabletypeService(SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);
        }

        public override string GetServiceTypeName() { return "Movabletype"; }

        public override bool CanEncode() { return false; }

        public override bool OpenSettingsForm(IWin32Window win)
        {
            bool result = false;
            MovabletypeForm form = new MovabletypeForm(this);
            if (form.ShowDialog() == DialogResult.OK)
            {
                result = true;
            }
            form.Dispose();

            return result;
        }
    }

    public class SixApartMovabletypeServiceCreator : BlogPhotoServiceCreator
    {
        public override BlogPhotoService GetNewService()
        {
            return new SixApartMovabletypeService();
        }
    }
}

