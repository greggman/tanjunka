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
    class TypePadForm : MovabletypeAPIForm
    {

        public TypePadForm(BlogPhotoService bps) :
            base(bps, "settings-typepadform.html", "TypePad Settings")
        {
        }

        public override bool AllowCustomXMLRPCURL { get { return false; } }
        public override bool AllowCustomImagePath { get { return false; } }
    }
}