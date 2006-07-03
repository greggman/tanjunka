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
    public class BloggerService : BloggerAPIService
    {
        public BloggerService()
        {
            PutEntry("xmlrpcURL", "http://www.blogger.com/api");
        }

        // deserialize constructor
        protected BloggerService(SerializationInfo info, StreamingContext context) : base (info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);
        }

        public override string GetServiceTypeName() { return "Blogger"; }
        public override bool CanEncode() { return false; }

        public override bool OpenSettingsForm(IWin32Window win)
        {
            bool result = false;
            BloggerForm bloggerForm = new BloggerForm(this);
            if (bloggerForm.ShowDialog() == DialogResult.OK)
            {
                result = true;
            }
            bloggerForm.Dispose();

            return result;
        }
        public override bool DisplayPostOptionsForm() { return false; }
        //--------------------- blogger only -----------


    }

    public class BloggerServiceCreator : BlogPhotoServiceCreator
    {
        public override BlogPhotoService GetNewService()
        {
            return new BloggerService();
        }
    }
}

