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
    class BlojsomForm : MovabletypeAPIForm
    {
        public BlojsomForm(BlogPhotoService bps) :
            base(bps, "settings-Blojsomform.html", "Blojsom Settings")
        {
        }
    }
}

