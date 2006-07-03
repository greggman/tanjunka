#region Using directives

using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Globalization;

#endregion

namespace Tanjunka
{
    public class TanLanguage : TanDictionary
    {
    }

    public class Localize
    {
        Dictionary<string, TanLanguage> _langs;

        TanLanguage _defaultLang; // language to use if current lang is missing entry
        TanLanguage _currentLang; // language user selected

        static Localize _theOnly;

        public static Localize G { get { return _theOnly; } }

        public static void Init ()
        {
            _theOnly = new Localize();
        }

        private Localize()
        {
            _langs = new Dictionary<string, TanLanguage>();

            // find the english file since we know it's there
            string defLangFile = "langs\\tanjunka-language-en-us.xml";
            if (!Util.FindLocalFile(ref defLangFile))
            {
                MessageBox.Show("Could not find language files!\n");
            }
            else
            {
                // read all of them
                string[] langFiles = Directory.GetFiles(Path.GetDirectoryName(defLangFile), "*.xml");

                foreach (string langFile in langFiles)
                {
                    TanLanguage lang = AddLanguage(langFile);
                    if (Path.GetFileName(langFile).CompareTo(Path.GetFileName(defLangFile)) == 0)
                    {
                        _defaultLang = lang;
                        _currentLang = lang;
                    }
                }
            }
        }

        private TanLanguage AddLanguage(string filename)
        {
            TanLanguage lang = new TanLanguage();

            lang.PutEntry("LangFilename", filename);

            // yea, I know I should maybe use the XML reader for this
            // but that's alot of knowledge I don't have and it will
            // throw exceptions left and right for stuff I don't
            // care about

            lang.PutEntry("language_create_time", File.GetLastWriteTime(filename).ToString(DateTimeFormatInfo.InvariantInfo));

            string str = Util.readFileToString(filename);

            Regex r = Util.MakeRegex("<Row.*?>.*?<Cell.*?>.*?<Data.*?>(?<1>.*?)</Data>.*?</Cell>.*?<Cell.*?>.*?<Data.*?>(?<2>.*?)</Data>.*?</Cell>", RegexOptions.IgnoreCase|RegexOptions.Compiled);
            MatchCollection mc = r.Matches(str);
            for (int ii = 0; ii < mc.Count; ii++)
            {
                Match m = mc[ii];

                if (lang.EntryExists(m.Groups[1].ToString()))
                {
                    Log.Printn("** duplicate entry for label(" + m.Groups[1].ToString() + ") in language file " + filename);
                }
                else
                {
                    lang.PutEntry(m.Groups[1].ToString(), Util.UnEntify(m.Groups[2].ToString()));
                }

                if (!lang.EntryExists("LanguageLabel"))
                {
                    Log.Printn("language file " + filename + "is missing language label\n");
                }
                else
                {
                    _langs[lang.GetEntry("LanguageLabel")] = lang;
                }
            }

            return lang;
        }

        public string GetText(string label)
        {
            if (_currentLang.EntryExists(label))
            {
                string str = _currentLang.GetEntry(label);
                if (str.Length > 0)
                {
                    return str;
                }
            }

            if (_defaultLang.EntryExists(label))
            {
                return _defaultLang.GetEntry(label);
            }

            Log.Printn("** missing language text **label(" + label + ") in language file " + _currentLang.GetEntry("LangFilename"));
            return "** missing text **";
        }

        public bool TextExists(string label)
        {
            return (_currentLang.EntryExists(label)) ||
                    (_defaultLang.EntryExists(label));
        }

        public void OpOnLanguages(ArrayListOperator alo)
        {
            foreach (KeyValuePair<string, TanLanguage> de in _langs)
            {
                if (!alo.operation(de.Value))
                {
                    return;
                }
            }
        }

        public bool SetLanguage()
        {
            string prefsLang = UserSettings.G.GetEntry("prefs_language");
            if (_currentLang.GetEntry("LanguageLabel").CompareTo(prefsLang) != 0)
            {
                if (_langs.ContainsKey(prefsLang))
                {
                    // remember the time the user set the language
                    UserSettings.G.PutEntry("language_set_time", System.DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo));
                    _currentLang = _langs[UserSettings.G.GetEntry("prefs_language")];
                    return true;
                }
                Log.Printn("** missing language **(" + prefsLang + ")");
            }
            return false;
        }

        private DateTime StringToDT(string tm)
        {
            if (tm.Length > 0)
            {
                return DateTime.Parse(tm, DateTimeFormatInfo.InvariantInfo);
            }

            return new DateTime(1980,1,1);
        }

        // this is the time the user set the language
        public DateTime GetLanguageSetTime()
        {
            return StringToDT(UserSettings.G.GetEntry("language_set_time"));
        }

        // this is the Write time of the language file for the current language
        public DateTime GetLanguageCreateTime()
        {
            return StringToDT(_currentLang.GetEntry("language_create_time"));
        }
    }
}
