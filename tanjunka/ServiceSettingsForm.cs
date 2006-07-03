#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

#endregion

namespace Tanjunka
{
    // form for editing moveable type service options
    partial class ServiceSettingsForm : TanHTMLForm
    {
        protected BlogPhotoService m_bps;

        public ServiceSettingsForm(BlogPhotoService bps, string formFilename, string title) :
            base(formFilename, title)
        {
            m_bps = bps;
        }

        protected void GetSetText(string label, bool bGetFromForm)
        {
            if (bGetFromForm)
            {
                // get from the form and put in C# struct
                m_bps.PutEntry(label ,GetText(label));
            }
            else
            {
                // get from C# struct and put in form
                SetText(label, m_bps.GetEntry(label));
            }
        }

        protected void GetSetCheck(string label, bool bGetFromForm)
        {
            if (bGetFromForm)
            {
                // get from the form and put in C# struct
                m_bps.PutEntry(label ,GetCheck(label));
            }
            else
            {
                // get from C# struct and put in form
                SetCheck(label, m_bps.GetEntry(label));
            }
        }

        protected override void GetSetAll (bool bGetFromForm)
        {
            GetSetText("servicename", bGetFromForm);
            if (m_bps.CanPostEntry())
            {
                GetSetCheck("usestylesheet",  bGetFromForm);
//                GetSetCheck("copystylesheet", bGetFromForm);
                GetSetCheck("useforms",       bGetFromForm);
                GetSetText("stylesheet",      bGetFromForm);
                GetSetText("bodyform",        bGetFromForm);
                GetSetText("moreform",        bGetFromForm);

                if (bGetFromForm)
                {
                    if (m_bps.CanEncode())
                    {
                        int codepage = int.Parse(GetValue("encoding"));
                        m_bps.PutIntEntry("encoding", codepage);
                    }
                }
                else
                {
                    if (m_bps.CanEncode())
                    {
                        ClearSelect("encoding");
                        int codepage = m_bps.GetIntEntry("encoding");
                        if (codepage == 0)
                        {
                            codepage = 65001; // UTF-8
                        }

                        // For every encoding, get the property values.
                        foreach( EncodingInfo ei in Encoding.GetEncodings())
                        {
                            Encoding e = ei.GetEncoding();

                            if (e.IsBrowserDisplay)
                            {
                                bool bSelected = (ei.CodePage == codepage);

                                AddOption("encoding", ei.DisplayName, ei.CodePage, bSelected);
                            }
                        }
                    }
                }
            }
            privGetSetAll(bGetFromForm);
        }

        protected virtual void privGetSetAll (bool bGetFromForm)
        {

        }

        private void BrowseFor(string str, string ext, string filter)
        {
            OpenFileDialog fd = new OpenFileDialog ();
            string filename = GetText(str);
            if (filename.Length > 0)
            {
                fd.FileName = filename;
                fd.InitialDirectory = Path.GetDirectoryName(filename);
            }

            fd.DefaultExt = ext;
            fd.Filter     = filter;
            fd.RestoreDirectory = true;
            fd.Title      = "Select " + str + "...";

            if (fd.ShowDialog() == DialogResult.OK)
            {
                SetText(str, fd.FileName);
            }
        }

        protected override void ClickSomething (int id)
        {
            switch (id)
            {
            case 13:    // stylesheet - browse
                BrowseFor("stylesheet", ".css", "CSS Files (*.css)|*.css|All files (*.*)|*.*");
                break;
            case 14:    // bodyform - browse
                BrowseFor("bodyform", ".html", "HTML Files(*.htm;*.html)|*.htm;*.html|All files (*.*)|*.*");
                break;
            case 15:    // moreform - browse
                BrowseFor("moreform", ".html", "HTML Files(*.htm;*.html)|*.htm;*.html|All files (*.*)|*.*");
                break;
            }
        }
    }
}