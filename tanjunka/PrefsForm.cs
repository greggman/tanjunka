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
    class PrefsForm : TanHTMLForm
    {
        private List<TanLanguage> _langs;

        public PrefsForm() :
            base("tan-PrefsForm.html", Localize.G.GetText("Prefs"))
        {
            _langs = new List<TanLanguage>();
        }

        protected void GetSetText(string label, bool bGetFromForm)
        {
            if (bGetFromForm)
            {
                // get from the form and put in C# struct
                UserSettings.G.PutEntry(label ,GetText(label));
            }
            else
            {
                // get from C# struct and put in form
                SetText(label, UserSettings.G.GetEntry(label));
            }
        }

        protected void GetSetCheck(string label, bool bGetFromForm)
        {
            if (bGetFromForm)
            {
                // get from the form and put in C# struct
                UserSettings.G.PutEntry(label ,GetCheck(label));
            }
            else
            {
                // get from C# struct and put in form
                SetCheck(label, UserSettings.G.GetEntry(label));
            }
        }

        class getLangs : ArrayListOperator
        {
            PrefsForm _pf;

            public getLangs(PrefsForm pf)
            {
                _pf = pf;
            }

            public override bool operation(Object ob)
            {
                TanLanguage lang = ob as TanLanguage;
                bool bSelected;

                if (UserSettings.G.GetEntry("prefs_language").Length == 0)
                {
                    bSelected = _pf._langs.Count == 0;
                }
                else
                {
                    bSelected = UserSettings.G.GetEntry("prefs_language").CompareTo(lang.GetEntry("LanguageLabel")) == 0;
                }
                _pf.AddOption("prefs_language", lang.GetEntry("LanguageLabel"), _pf._langs.Count, bSelected);
                _pf._langs.Add(lang);

                return true;
            }
        }

        protected override void GetSetAll (bool bGetFromForm)
        {
            GetSetText ("prefs_maxwidth",     bGetFromForm);
            GetSetText ("prefs_maxheight",    bGetFromForm);
            GetSetCheck("prefs_showhtml",     bGetFromForm);
            GetSetCheck("prefs_autogennames", bGetFromForm);
            GetSetCheck("proxy_useproxy",     bGetFromForm);
            GetSetText ("proxy_address",      bGetFromForm);
            GetSetCheck("proxy_useauth",      bGetFromForm);
            GetSetText ("proxy_username",     bGetFromForm);
            GetSetText ("proxy_password",     bGetFromForm);
            GetSetCheck("prefs_showlog",      bGetFromForm);
            GetSetCheck("prefs_showdebugmenu",bGetFromForm);

            if (bGetFromForm)
            {
                string langid = GetValue("prefs_language");
                if (langid.Length > 0)
                {
                    int langNdx = int.Parse(langid);
                    UserSettings.G.PutEntry("prefs_language", _langs[langNdx].GetEntry("LanguageLabel"));
                }
            }
            else
            {
                ClearSelect("prefs_language");
                _langs.Clear();
                Localize.G.OpOnLanguages(new getLangs(this));
            }
        }
    }
}