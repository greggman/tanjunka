#region Using directives

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

using CookComputing.XmlRpc;

#endregion


namespace Tanjunka
{
    [Serializable]
    public class TypePadService : MovabletypeService
    {
        public TypePadService()
        {
            PutEntry("xmlrpcURL", "http://www.typepad.com/t/api");
        }

        // deserialize constructor
        protected TypePadService(SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);
        }

        public override string GetServiceTypeName() { return "TypePad"; }
        public override bool CanEncode() { return false; }

        public override bool OpenSettingsForm(IWin32Window win)
        {
            bool result = false;
            TypePadForm form = new TypePadForm(this);
            if (form.ShowDialog() == DialogResult.OK)
            {
                result = true;
            }
            form.Dispose();

            return result;
        }
    }

    public class TypePadServiceCreator : BlogPhotoServiceCreator
    {
        public override BlogPhotoService GetNewService()
        {
            return new TypePadService();
        }
    }
}
