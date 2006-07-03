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
    class MovabletypeForm : MovabletypeAPIForm
    {
        public MovabletypeForm(BlogPhotoService bps) :
            base(bps, "settings-movabletypeform.html", "Movabletype Settings")
        {
        }
    }
}