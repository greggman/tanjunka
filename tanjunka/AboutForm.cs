#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;

#endregion

namespace Tanjunka
{
    // form for editing moveable type service options
    class AboutForm : TanHTMLForm
    {
        public AboutForm() :
            base("tan-AboutForm.html", Localize.G.GetText("AboutTitle"))
        {
        }

        protected override void GetSetAll(bool bGetFromForm)
        {
            if (!bGetFromForm)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo FVI = FileVersionInfo.GetVersionInfo(assembly.Location);

                SetInnerText("version", Util.GetVersion());
            }
        }
    }
}

