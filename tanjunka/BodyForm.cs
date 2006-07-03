#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using CookComputing.XmlRpc;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

#endregion

namespace Tanjunka
{

    partial class BodyForm : Form
    {
        public static string[] htmlParts = { "postbody", "postextra", };

        private PersistWindowState m_windowState;

        private NetSpell.SpellChecker.Spelling spelling;
        private NetSpell.SpellChecker.Dictionary.WordDictionary wordDictionary;
        private StringBuilder textBeingSpellChecked;

        private bool m_allowBodyUpdate = true;     // hack to prevent webbrowser controls from navigating but still allow us to update them
        private bool m_allowExtraUpdate = true;    // hack to prevent webbrowser controls from navigating but still allow us to update them
        private bool m_allowOptionsUpdate = true;  // hack to prevent webbrowser controls from navigating but still allow us to update them
        private bool m_supressEvents; // turns off events

        private EntryInfo m_entry;    // current entry
        private Dictionary<string, BlogPhotoServiceCreator> m_services; // this is the list of all services

        private TanTabPage m_curTab;
        private PublishStatusForm m_psf;
        private PostSet m_curPS;

        private SpellCheckForm suggestionForm;

        // ---------------------------- form -----------------------------------------

        public BodyForm(string arg)
        {
            m_supressEvents = false;

            Util.InitCache();
            Localize.Init();

            m_services = new Dictionary<string, BlogPhotoServiceCreator>();

            m_services["AtomAPI"]      = new AtomAPIServiceCreator();
//            m_services["b2evolution"]  = new b2evolutionServiceCreator();
//            m_services["Blog:CMS"]     = new BlogCMSServiceCreator();
            m_services["Blogger"]      = new BloggerServiceCreator();
            m_services["Blogger API"]  = new BloggerAPIServiceCreator();
            m_services["Blojsom"]      = new BlojsomServiceCreator();
//            m_services["Durpal"]       = new DrupalServiceCreator();
            m_services["Flickr"]       = new FlickrServiceCreator();
            m_services["GMan"]         = new GManServiceCreator();
            m_services["Live Journal"] = new LiveJournalServiceCreator();
//            m_services["Manilla"]      = new ManillaServiceCreator();
            m_services["MovableType"]  = new SixApartMovabletypeServiceCreator();
            m_services["MovableType API"]  = new MovabletypeServiceCreator();
            m_services["Nucleus"]      = new NucleusServiceCreator();
//            m_services["pMachine"]     = new SquareSpaceServiceCreator();
//            m_services["Squarespace"]  = new SquareSpaceServiceCreator();
            m_services["TypePad"]      = new TypePadServiceCreator();
            m_services["WordPress"]    = new WordPressServiceCreator();

            m_entry = new EntryInfo();

            UserSettings.Load();
            Localize.G.SetLanguage();

            InitializeComponent();
            InitializeComponentPhotoEdit();
            m_windowState = new PersistWindowState(this);

            this.spelling = new NetSpell.SpellChecker.Spelling(this.components);
            this.wordDictionary = new NetSpell.SpellChecker.Dictionary.WordDictionary(this.components);

            this.spelling.Dictionary = this.wordDictionary;
            this.spelling.IgnoreHtml = true;
            this.spelling.ShowDialog = false;
            this.spelling.ReplacedWord += new NetSpell.SpellChecker.Spelling.ReplacedWordEventHandler(this.spelling_ReplacedWord);
            this.spelling.EndOfText    += new NetSpell.SpellChecker.Spelling.EndOfTextEventHandler(this.spelling_EndOfText);
            this.spelling.DeletedWord  += new NetSpell.SpellChecker.Spelling.DeletedWordEventHandler(this.spelling_DeletedWord);

            {
                string dicFolder = Path.Combine(Util.GetLocalDirectory(), "dic");

                string dicFilename = UserSettings.G.GetEntry("prefs_dictionary");
                if (dicFilename.Length == 0)
                {
                    dicFilename = Thread.CurrentThread.CurrentCulture.Name + ".dic";
                }
                if (!File.Exists(Path.Combine(dicFolder, dicFilename)))
                {
                    dicFilename = "en-us.dic";
                }

                this.wordDictionary.DictionaryFile = Path.GetFileName(dicFilename);
                this.wordDictionary.DictionaryFolder = dicFolder;
            }

            this.suggestionForm = new SpellCheckForm(this.spelling);
            this.suggestionForm.VisibleChanged += new EventHandler(spelling_Closed);

            syncPostButtons();
            UpdateVisualPrefs();

            m_curTab = this.tabBody;

            // figure out the default service to display
            // and add current services to postset list
            m_supressEvents = true;
            UpdatePostSets();
            m_supressEvents = false;

//            this.WebBody.KeyPress += new System.Windows.Forms.KeyEventHandler(this.BodyForm_KeyPress);
//            this.WebBody.KeyDown  += new System.Windows.Forms.KeyDownEventHandler(this.BodyForm_KeyDown);
//            this.WebBody.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BodyForm_KeyPress);
//            this.WebBody.KeyDown  += new System.Windows.Forms.KeyEventHandler(this.BodyForm_KeyDown);

            this.wbBody.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(this.body_docCompleted);
            this.wbExtra.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(this.extra_docCompleted);
            this.wbOptions.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(this.options_docCompleted);

            this.tabBody.SetUpdateEntryFunc(update_fromBody);
            this.tabBody.SetUpdatePageFunc(update_bodyPage);
            this.tabBody.SetExecCommandFunc(execCommand_body);
            this.tabBody.SetUICommandFunc(UICommand_body);
            this.tabBody.SetQueryCommandValueFunc(queryCommandValue_body);
            this.tabBody.SetInsertHTMLFunc(insertHTML_body);

            this.tabExtra.SetUpdateEntryFunc(update_fromExtra);
            this.tabExtra.SetUpdatePageFunc(update_extraPage);
            this.tabExtra.SetExecCommandFunc(execCommand_extra);
            this.tabExtra.SetUICommandFunc(UICommand_extra);
            this.tabExtra.SetQueryCommandValueFunc(queryCommandValue_extra);
            this.tabExtra.SetInsertHTMLFunc(insertHTML_extra);

            this.tabImage.SetUpdateEntryFunc(update_entryFromImage);
            this.tabImage.SetUpdatePageFunc(update_ImageFromEntry);
            this.tabImage.SetUICommandFunc(UICommand_image);

            this.tabBodyHTML.SetUpdateEntryFunc(update_fromBodyHTML);
            this.tabBodyHTML.SetUpdatePageFunc(update_bodyHTMLPage);

            this.tabExtraHTML.SetUpdateEntryFunc(update_fromExtraHTML);
            this.tabExtraHTML.SetUpdatePageFunc(update_extraHTMLPage);

            this.tabOptions.SetUpdateEntryFunc(update_fromOptions);
            this.tabOptions.SetUpdatePageFunc(update_optionsPage);

//            this.tabOptions
//            this.tabExtraHTML
//            this.tabLog


            tabBright_realResize();
            tabHSV_realResize();

            // init styles
            {
                Object[] SITable = new Object[] {
                    new StyleInfo("Heading1", "<h1>"),
                    new StyleInfo("Heading2", "<h2>"),
                    new StyleInfo("Heading3", "<h3>"),
                    new StyleInfo("Heading4", "<h4>"),
                    new StyleInfo("Heading5", "<h5>"),
                    new StyleInfo("Heading6", "<h6>"),
                    new StyleInfo("Preformatted", "<pre>"),
                    new StyleInfo("Normal", "<p>"),
                };
                this.styleToolStripComboBox.Items.AddRange(SITable);
                this.styleToolStripComboBox.ComboBox.DisplayMember = "label";
                m_supressEvents = true;
                this.styleToolStripComboBox.SelectedIndex = 7;
                m_supressEvents = false;
            }

            // init fontsizes
            {
                string[] sizes = new string[] { "1", "2", "3", "4", "5", "6", "7" };
                this.fontSizeToolStripComboBox.Items.AddRange(sizes);
                m_supressEvents = true;
                this.fontSizeToolStripComboBox.SelectedIndex = 1;
                m_supressEvents = false;
            }

            {
                ColorInit[] CITable = new ColorInit[]{
                    new ColorInit( "Red", Color.Red ),
                    new ColorInit( "Blue", Color.Blue),
                    new ColorInit( "Green", Color.Green),
                    new ColorInit( "Black", Color.Black),
                    new ColorInit( "Yellow", Color.Yellow),
                    new ColorInit( "Purple", Color.Purple),
                    new ColorInit( "Orange", Color.Orange),
                    new ColorInit( "Cyan", Color.Cyan),
                    };

                // init fontColor
                {
                    foreach (ColorInit ci in CITable)
                    {
                        //ToolStripMenuItem b1 = new ToolStripMenuItem();
                        for (int ii = 0; ii < 2; ++ii)
                        {
                            TanColorMenuItem b1 = new TanColorMenuItem(ci.tanColor, ci.tanColorID);

                            b1.Text = "";
                            b1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
                            b1.Width = 20;

                            if (ii == 0)
                            {
                                b1.Click += new System.EventHandler(this.fontColorSet_Click);
                                this.fontColorToolStripSplitButton.DropDownItems.Add(b1);
                            }
                            else
                            {
                                b1.Click += new System.EventHandler(this.highlightSet_Click);
                                this.highlightToolStripSplitButton.DropDownItems.Add(b1);
                            }
                        }
                    }
                    this.fontColorToolStripSplitButton.TanColor = Color.Red;
                    this.fontColorToolStripSplitButton.TanColorID = "Red";
                    this.highlightToolStripSplitButton.TanColor = Color.Yellow;
                    this.highlightToolStripSplitButton.TanColorID = "Yellow";
                }
            }

            Log.Init(this.log);

            if (arg.Length > 0)
            {
                // is there an & in the args?
                if (arg.IndexOf('&') >= 0)
                {
                    // yes, then probably from trackback
                    MessageBox.Show(this, arg);
                }
                else
                {
                    // it's just a regular file
                    loadEntry (arg);
                }
            }
        }

        private void loadEditForm()
        {
            m_allowBodyUpdate = true;
            m_allowExtraUpdate = true;
            m_allowOptionsUpdate = true;

            Util.SetFormHTML(this.wbOptions, (m_curPS != null) ? m_curPS.GetBlogService().GetOptionForm() : "tan-nooptions.html");
            Util.SetFormHTML(this.wbBody, "tan-bodyform.html");
            Util.SetFormHTML(this.wbExtra, "tan-moreform.html");
        }

        // Configures WebBrowser1 so script code can access the form.
        private void Form1_Load(object sender, System.EventArgs e)
        {
            this.wbBody.setBox(this.log);
            this.wbExtra.setBox(this.log);
            this.wbOptions.setBox(this.log);

            this.wbBody.setGenericCallbackI(new myVoidMethodDelegateI(FormCommand));
            this.wbBody.setGenericCallbackISII_S(new myVoidMethodDelegateISII_S(ComplexCallback));
            this.wbBody.setGenericCallbackISS_S(new myVoidMethodDelegateISS_S(JScriptInfoRequest));
            this.wbExtra.setGenericCallbackI(new myVoidMethodDelegateI(FormCommand));
            this.wbExtra.setGenericCallbackISII_S(new myVoidMethodDelegateISII_S(ComplexCallback));
            this.wbExtra.setGenericCallbackISS_S(new myVoidMethodDelegateISS_S(JScriptInfoRequest));
            this.wbOptions.setGenericCallbackI(new myVoidMethodDelegateI(OptionsFormCommand));

            loadEditForm();
        }

        private void UpdateVisualPrefs()
        {
            this.tabMainTabs.SuspendLayout();
            this.tabMainTabs.Visible = false;
            this.tabMainTabs.Controls.Clear();

            List<TabPage> pages = new List<TabPage>();

            pages.Add(this.tabBody);
            pages.Add(this.tabExtra);
            pages.Add(this.tabImage);
            pages.Add(this.tabOptions);

            if (UserSettings.G.GetBoolEntry("prefs_showhtml"))
            {
                pages.Add(this.tabBodyHTML);
                pages.Add(this.tabExtraHTML);
            }
            if (UserSettings.G.GetBoolEntry("prefs_showlog"))
            {
                pages.Add(this.tabLog);
            }
            if (UserSettings.G.GetBoolEntry("prefs_showdebugmenu"))
            {
                if (!this.menuStrip1.Items.Contains(debugToolStripMenuItem))
                {
                    this.menuStrip1.Items.Add(debugToolStripMenuItem);
                }
            }
            else
            {
                if (this.menuStrip1.Items.Contains(debugToolStripMenuItem))
                {
                    this.menuStrip1.Items.Remove(debugToolStripMenuItem);
                }
            }

            TabPage[] pageArray = new TabPage[pages.Count];

            int ii = 0;
            foreach (TabPage tp in pages)
            {
                pageArray[ii] = tp;
                ii++;
            }

            this.tabMainTabs.Controls.AddRange(pageArray);
            this.tabMainTabs.Visible = true;
            this.tabMainTabs.ResumeLayout(false);

//            this.tabBodyHTML.Visible  = UserSettings.G.GetBoolEntry("prefs_showhtml");
//            this.tabExtraHTML.Visible = UserSettings.G.GetBoolEntry("prefs_showhtml");
//            this.tabLog.Visible       = UserSettings.G.GetBoolEntry("prefs_showlog");
//            this.debugToolStripMenuItem.Visible = UserSettings.G.GetBoolEntry("prefs_showdebugmenu");

            if (UserSettings.G.GetBoolEntry("proxy_useproxy"))
            {
                Uri proxyURI   = new Uri(UserSettings.G.GetEntry("proxy_address"));
                WebProxy proxy = new WebProxy(proxyURI, true);

                if (UserSettings.G.GetBoolEntry("proxy_useauth"))
                {
                    proxy.Credentials = new NetworkCredential(
                        UserSettings.G.GetEntry("proxy_username"),
                        UserSettings.G.GetEntry("proxy_password"));
                }

                //GlobalProxySelection.Select = proxy; // 2.0 beta 1
                WebRequest.DefaultWebProxy = proxy; // 2.0 beta 2
            }
            else
            {
                // GlobalProxySelection.Select = GlobalProxySelection.GetEmptyWebProxy(); // 2.0 beta 1
                WebRequest.DefaultWebProxy = null; // 2.0 beta 2
            }
        }

        class turnOffAllPostSets : ArrayListOperator
        {
            public turnOffAllPostSets()
            {
            }

            public override bool operation (Object ob)
            {
                PostSet ps = ob as PostSet;

                ps.SetOn(false);

                return true;
            }
        }


        class findDefPostSetOp : ArrayListOperator
        {
            private ToolStripComboBox   m_cb;
            private PostSet             m_ps;

            public PostSet          FirstOnPostSet;
            public PostSet          FirstPostSet;
            public bool             bFoundCurPS;

            public findDefPostSetOp(ToolStripComboBox cb, PostSet ps)
            {
                m_cb = cb;
                m_ps = ps;
                FirstOnPostSet = null;
                FirstPostSet = null;
                bFoundCurPS = false;

                cb.Items.Clear();
            }

            public override bool operation (Object ob)
            {
                PostSet ps = (PostSet)ob;

                if (FirstPostSet == null)
                {
                    FirstPostSet = ps;
                }

                if (ps.IsOn())
                {
                    if (FirstOnPostSet == null)
                    {
                        FirstOnPostSet = ps;
                    }
                }
                m_cb.Items.Add(ps);

                if (ps == m_ps)
                {
                    bFoundCurPS = true;
                }
                return true;
            }
        }



        // update the postset combobox as well as switch to a valid default postset
        // for options etc if we don't have one.
        private void UpdatePostSets()
        {
            findDefPostSetOp f = new findDefPostSetOp(this.postSetToolStripComboBox, m_curPS);
            UserSettings.G.OpOnPostSets(f);
            PostSet ps = null;

            // use the first on set if there is one
            // else use the first set
            if (f.FirstOnPostSet != null)
            {
                ps = f.FirstOnPostSet;
            }
            else
            {
                ps = f.FirstPostSet;
            }

            // if the current set does not exist
            if (!f.bFoundCurPS)
            {
                // clear them
                m_curPS = null;

                // if we found a set make it the new set
                if (ps != null)
                {
                    m_curPS  = ps;
                }
            }

            if (this.postSetToolStripComboBox.Items.Count == 0)
            {
                this.postSetToolStripComboBox.Items.Add("*no postsets*");
                this.postSetToolStripComboBox.SelectedIndex = 0;
            }
            else
            {
                this.postSetToolStripComboBox.SelectedItem = m_curPS;
            }
        }

        // call to add a file returning the html needed to insert it
        // (ie, insert a link to it)
        private string AddFile(string filename)
        {
            // do different things depending on what it is
            string ext = Path.GetExtension(filename).ToLower();
            string textToInsert = "";

            if (TJPagePicture.CanHandle(ext))
            {
                Log.Printn("add picture (" + filename + ")");
                textToInsert = m_entry.AddPicture(filename);
            }
            else if (ext.CompareTo(".txt") == 0)
            {
                Log.Printn("add text (" + filename + ")");
            }
            else if (ext.CompareTo(".htm") == 0 ||
                     ext.CompareTo(".html") == 0)
            {
                Log.Printn("add html (" + filename + ")");
                // if it's an http: it's probably a link
                if (String.Compare(filename.Substring(5), "http:") == 0)
                {
                    textToInsert = "<a href=\" + filename + \">" + filename + "</a>";
                }
            }
            else
            {
                Log.Printn("add *unknown* (" + filename + ")");
            }

            return textToInsert;
        }

        // Halts navigation when the user drags and drops a file here
        // also tells us what the file was!  Awesome!
        private void WebBody_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e)
        {
            Log.Printn("--navigating--");
            if (!m_allowBodyUpdate)
            {
                Log.Printn("--allowBodyUpdate--");
                TanWebBrowser wb = (TanWebBrowser)sender;
                e.Cancel = true;
                Log.Printn ("navigating(" + e.Url + ")");
                string textToInsert = AddFile(e.Url.LocalPath);

                if (textToInsert.Length > 0)
                {
                    Log.Printn("inserting (" + textToInsert + ")");
                    /*
                    string old = wb.Document.All["postbody"].GetAttribute("innerText");
                    wb.Document.All["postbody"].SetAttribute("innerText", textToInsert + old);
                    */

                    // this **SHOULD** work but for some reason once the text is added
                    // alphanumeric keys no longer input.  DEL still works though?
                    /*
                    string old = wb.Document.All["postbody"].InnerHtml;
                    wb.Document.All["postbody"].InnerHtml = textToInsert + old;
                    */

                    // this work around seems to work although I hope this is
                    // safe.  I'm not totally familar with C#

                    Object[] args = new Object[2] { "postbody", textToInsert };
                    wb.Document.InvokeScript("insertHTML", args);

                    //todo: maybe we can call "insertHTML" and it
                    // can check if there is a selection or it can
                    // save and create one
                    //curTab.ExecCommand("Paste", textToInsert);

                }
            }
            m_allowBodyUpdate = false;
        }

        private void webBrowser2_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e)
        {
            if (!m_allowExtraUpdate)
            {
                TanWebBrowser wb = (TanWebBrowser)sender;
                e.Cancel = true;
                Log.Printn ("navigating2(" + e.Url + ")");
                string textToInsert = AddFile(e.Url.ToString());

                if (textToInsert.Length > 0)
                {
                    Log.Printn("inserting2 (" + textToInsert + ")");
                    /*
                    string old = wb.Document.All["postbody"].GetAttribute("innerText");
                    wb.Document.All["postbody"].SetAttribute("innerText", textToInsert + old);
                    */

                    // this **SHOULD** work but for some reason once the text is added
                    // alphanumeric keys no longer input.  DEL still works though?
                    /*
                    string old = wb.Document.All["postbody"].InnerHtml;
                    wb.Document.All["postbody"].InnerHtml = textToInsert + old;
                    */

                    // this work around seems to work although I hope this is
                    // safe.  I'm not totally familar with C#

                    Object[] args = new Object[2] { "postbody", textToInsert };
                    wb.Document.InvokeScript("insertHTML", args);

                    //todo: maybe we can call "insertHTML" and it
                    // can check if there is a selection or it can
                    // save and create one
                    //curTab.ExecCommand("Paste", textToInsert);

                }
            }
            m_allowExtraUpdate = false;
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (!m_allowOptionsUpdate)
            {
                e.Cancel = true;
            }
            m_allowOptionsUpdate = false;
        }


    /*
        if (this.WebBody.Document.All["userName"].
            GetAttribute("value").Equals(""))
        {
            e.Cancel = true;
            System.Windows.Forms.MessageBox.Show(
                "Please enter your name.");
        }
    }
    */


/*
    // Halts navigation if the user has not entered a name.
    private void WebBrowser1_Navigating(object sender,
        System.Windows.Forms.WebBrowserNavigatingEventArgs e)
    {
        if (this.WebBrowser1.Document.All["userName"].
            GetAttribute("value").Equals(""))
        {
            e.Cancel = true;
            System.Windows.Forms.MessageBox.Show(
                "Please enter your name.");
        }
    }
*/

        private void AddImagesInHTMLToUpload()
        {
            m_entry.ClearHtmlImageList();

            {
                // find all <img src="">
                Regex r1 = Util.MakeRegex("(?<1><img\\s+)(?<2>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[^>\"'/])*)(?<3>/*>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                foreach (string part in htmlParts)
                {
                    // Replace matched characters using the delegate method.
                    MatchCollection mc = r1.Matches(m_entry.GetEntry(part));

                    for (int ii = 0; ii < mc.Count; ii++)
                    {
                        Match m = mc[ii];

                        string middle = m.Groups[2].ToString();

                        // find the "src"
                        Regex r = Util.MakeRegex("src\\s*=\\s*(?<1>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*))", RegexOptions.IgnoreCase|RegexOptions.Compiled);
                        Match srcMatch = r.Match(middle);
                        if (srcMatch.Groups.Count > 1)
                        {
                            string src = Util.RemoveQuotes(srcMatch.Groups[1].ToString());

                            if (m_entry.PictureTempExists(src))
                            {
                                TJPagePicture tpic = m_entry.GetTempPicture(src);

                                m_entry.AddImageToHtmlImageList(tpic);
                            }
                        }
                    }
                }
            }

            // -- I'm pretty sure this FUCKs up the editing
            // get a list of the images in this post
            #if false
            for (int ii = 0; ii < this.WebBody.Document.Images.Count; ++ii)
            {
                string imageSrc = this.WebBody.Document.Images[ii].GetAttribute("src");
                Log.Printn(imageSrc);

                if (entry.PictureTempExists(imageSrc))
                {
                    TJPagePicture tpic = entry.GetTempPicture(imageSrc);

                    entry.AddImageToHtmlImageList(tpic);
                }
            }
            #endif
        }

        public static void bs_refresh()
        {
//            Application.DoEvents();
        }

        public void syncEntryFromForm()
        {
            if (m_curTab != null)
            {
                m_curTab.UpdateEntry(m_entry);
            }
        }

        public void syncFormFromEntry()
        {
            if (m_curTab != null)
            {
                m_curTab.UpdatePage(m_entry);
            }
        }

        private void newpost_Click(object sender, EventArgs e)
        {
            m_entry.PutIntEntry("postCount", m_entry.GetIntEntry("postCount") + 1);
            publish_Click(sender, e);
        }

        private void republish_Click(object sender, EventArgs e)
        {
            publish_Click(sender, e);
        }

        private void publish_Click(object sender, EventArgs e)
        {
            if (UserSettings.G.GetNumPostSets() > 1 && UserSettings.G.GetNumOnPostSets() > 0)
            {
                OpenPublishForm();
            }
            else
            {
                publish_for_real();
            }
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Dib, false) == true)
            {
                Log.Printn ("--dropped DIB--");

                if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    // loop through the string array, adding each filename to the ListBox

                    foreach (string file in files)
                    {
                        Log.Printn (file);
                    }
                }
                else
                {
                    Log.Printn ("*no filedrop info for dib");
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                // get the filenames
                // (yes, everything to the left of the "=" can be put in the
                // foreach loop in place of "files", but this is easier to understand.)

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped file--");
                foreach (string file in files)
                {
                    Log.Printn (file);
                }
            }
            else if (e.Data.GetDataPresent("UniformResourceLocator", false) == true)
            {
                Log.Printn ("--dropped URL--");
                {
                    string link = (string)e.Data.GetData(DataFormats.UnicodeText);

                    Log.Printn ("part1:" + link);
                }
                {
                    System.IO.Stream ioStream =
                    (System.IO.Stream)e.Data.GetData("FileGroupDescriptor");
                    byte[] contents = new Byte[2048];
                    ioStream.Read(contents, 0, 2048);
                    ioStream.Close();
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    //The magic number 76 is the size of that part of the
                    //FILEGROUPDESCRIPTOR structure before
                    // the filename starts - cribbed
                    //from another usenet post.
                    for (int i = 76; contents[i] != 0; i++)
                    {
                        sb.Append((char)contents[i]);
                    }
                    if (!sb.ToString(sb.Length - 4, 4).ToLower().Equals(".url"))
                    {
                        throw new Exception("filename does not end in '.url'");
                    }
                    string link = sb.ToString(0, sb.Length - 4);

                    Log.Printn ("part2:" + link);
                }

            }
            else if (e.Data.GetDataPresent(DataFormats.SymbolicLink, false) == true)
            {
                string link = (string)e.Data.GetData(DataFormats.SymbolicLink);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped link--");
                Log.Printn (link);
            }
            else if (e.Data.GetDataPresent(DataFormats.Html, false) == true)
            {
                string link = (string)e.Data.GetData(DataFormats.Html);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped html--");
                Log.Printn (link);
            }
            else if (e.Data.GetDataPresent(DataFormats.Rtf, false) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.Rtf);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped rtf--");
                foreach (string file in files)
                {
                    Log.Printn (file);
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.UnicodeText, false) == true)
            {
                string unitext = (string)e.Data.GetData(DataFormats.UnicodeText);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped unicode--");
                Log.Printn (unitext);
            }
            else if (e.Data.GetDataPresent(DataFormats.Text, true) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.Text);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped text--");
                foreach (string file in files)
                {
                    Log.Printn (file);
                }
            }
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            // make sure they're actually dropping files (not text or anything else)
            {
                string[] formats = e.Data.GetFormats();

                Log.Printn ("---formats---");
                foreach (string format in formats)
                {
                    Log.Printn (format);
                }
            }

            if (
                (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Bitmap, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Dib, false) == true) ||
                (e.Data.GetDataPresent("UniformResourceLocator", false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Html, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Rtf, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.UnicodeText, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.SymbolicLink, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Text, true) == true) ||
                false
                )
            {
                // allow them to continue
                // (without this, the cursor stays a "NO" symbol
                e.Effect = DragDropEffects.All;
            }
        }

        private void reloadeditformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadEditForm();
        }

        private void showBlogInfoFormToolStripMenuItem_Click(object sender, EventArgs e)
        {
//            blogForm.ShowDialog();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_entry.cleanupTempFiles();
        }

        private void OpenPublishForm()
        {
            PublishForm publishForm = new PublishForm(m_services);
            if (publishForm.ShowDialog(this) == DialogResult.OK)
            {
                publish_for_real();
            }
            publishForm.Dispose();
            UpdatePostSets();
        }

        private void publistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenPublishForm();
        }

        private void postSetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenPublishForm();
        }

        private void servicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ServicesForm servicesForm = new ServicesForm(m_services);
            servicesForm.ShowDialog(this);
            servicesForm.Dispose();
            UpdatePostSets();
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syncEntryFromForm();

            PrefsForm form = new PrefsForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // if the language changed
                if (Localize.G.SetLanguage())
                {
                    UpdateUILanguage();
                    syncFormFromEntry();
                }
                UpdateVisualPrefs();
            }
            form.Dispose();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm form = new AboutForm();
            form.ShowDialog(this);
            form.Dispose();
        }


        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("C:\\Program Files\\Common Files\\Thraex Software\\AutoUpdator\\AutoUpdator.exe"); //, "/author \"Greggman\" /program \"Tanjunka\"");
            }
            catch (Exception ex)
            {
                Log.Printn("could not start updater.  Please choose it from the Start Menu\n" + ex.Message);
            }
        }

        private void forumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://tanjunka.com/tan/forum/");
            }
            catch (Exception ex)
            {
                Log.Printn("could not go to forum\n" + ex.Message);
            }
        }

        private void webpageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://tanjunka.com/");
            }
            catch (Exception ex)
            {
                Log.Printn("could not go to webpage\n" + ex.Message);
            }
        }

        private void spellcheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syncEntryFromForm();

            m_curTab.UICommand(TanTabPage.UICommandID.SpellCheckStart);
            this.spelling.Text = textBeingSpellChecked.ToString();
            this.Enabled = false;
            try
            {
                this.spelling.SpellCheck();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start spell checker\n" + ex.Message);
                this.Enabled = true;
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string helpfile = "help/index.html";
                if (Util.FindLocalFile(ref helpfile))
                {
                    System.Diagnostics.Process.Start(Path.GetFullPath(helpfile));
                }
                else
                {
                    Log.Printn("could not display help\n" + helpfile);
                }
            }
            catch (Exception ex)
            {
                Log.Printn("could not display help\n" + ex.Message);
            }
        }

        private bool verifyIfModified(string msg)
        {
            m_curTab.UpdateEntry(m_entry);
            // is the body in the editor different than
            // the body stored?
            if (m_entry.IsModified())
            {
                if (MessageBox.Show(
                        msg,
                        Localize.G.GetText("LabelConfirmation"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return false;
                }
            }
            return true;
        }

        private void event_newDoc(object sender, EventArgs e)
        {
            if (verifyIfModified(Localize.G.GetText("PromptClearIt")))
            {
                // should I actually clear the old one instead
                m_entry.Dispose();
                m_entry = new EntryInfo();
                SetEditImage(null);
            }
            updateForms();
        }

        private void syncPostButtons()
        {
            if (m_entry.GetEntry("__published__").Length > 0)
            {
                this.newpostButton.Enabled = true;
                this.republishButton.Enabled = true;
                this.publishButton.Enabled = false;
            }
            else
            {
                this.newpostButton.Enabled = false;
                this.republishButton.Enabled = false;
                this.publishButton.Enabled = true;
            }
        }

        private void updateForms()
        {
            update_bodyPage(m_entry);
            update_extraPage(m_entry);
            update_optionsPage(m_entry);
            syncPostButtons();
        }

        private string RemapSRC(Match m)
        {
            string middle = m.Groups[2].ToString();

            // find the "src"
            // find all <img src="">
            Regex r = Util.MakeRegex("src\\s*=\\s*(?<1>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*))", RegexOptions.IgnoreCase|RegexOptions.Compiled);

            Match srcMatch = r.Match(middle);
            if (srcMatch.Groups.Count > 1)
            {
                string src = Util.RemoveQuotes(srcMatch.Groups[1].ToString());

                if (m_entry.PictureTempExists(src))
                {
                    TJPagePicture tjpic = m_entry.GetTempPicture(src);

                    string newSrc    = tjpic.GetNewTempPath();

                    Regex r1 = Util.MakeRegex("src\\s*=\\s*(?<1>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*))", RegexOptions.IgnoreCase|RegexOptions.Compiled);
                    middle = r1.Replace(middle, "src=\"" + newSrc + "\"");
                }
            }

            return m.Groups[1].ToString() + middle + " />";  //m.Groups[3].ToString();
        }

        private void loadEntry(string filename)
        {
            if (verifyIfModified(Localize.G.GetText("PromptLoadEntry")))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    m_entry.Dispose();
                    int version = (int)formatter.Deserialize(stream);
                    EntryInfo.SetLoadVersion(version);
                    m_entry = (EntryInfo)formatter.Deserialize(stream);
                    SetEditImage(null);

                    // now we need to go through and make all the <img> tags
                    // point to the New temps
                    foreach (string part in htmlParts)
                    {
                        string bod = m_entry.GetEntry(part);
                        // find all <img src="">
                        Regex r = Util.MakeRegex("(?<1><img\\s+)(?<2>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[^>\"'/])*)(?<3>/*>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                        // Replace matched characters using the delegate method.
                        bod = r.Replace(bod, new MatchEvaluator(this.RemapSRC));
                        m_entry.PutEntryNoMod(part, bod);
                    }
                    m_entry.FixupNewVsOldTempPaths();
                }
                catch (Exception ex)
                {
                    Log.Printn("EX: " + ex.Message);
                    MessageBox.Show(Localize.G.GetText("ErrorTroubleReadingEntry"));
                    m_entry = new EntryInfo();
                }
                stream.Close();
                updateForms();
            }
        }

        private void saveEntry()
        {
            syncEntryFromForm();

            // when saving we need to
            //  1) copy the source and temp images into the file

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(m_entry.GetEntry("filename"), FileMode.Create, FileAccess.Write, FileShare.None);
            int version = EntryInfo.GetVersion();
            formatter.Serialize(stream, version);
            formatter.Serialize(stream, m_entry);
            stream.Close();
        }

        private void event_openDoc(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog ();

            string filename = m_entry.GetEntry("filename");
            // TODO: use userSettings for directory if no file
            if (filename.Length > 0)
            {
                fd.FileName = filename;
                fd.InitialDirectory = Path.GetDirectoryName(filename);
            }
            else
            {
                fd.InitialDirectory = UserSettings.G.GetEntry("openDocPath");
            }

            fd.DefaultExt = ".tan";
            fd.Filter     = "Tanjunka files (*.tan)|*.tan|All files (*.*)|*.*";
            fd.RestoreDirectory = true;
            fd.Title      = "Open...";

            if (fd.ShowDialog() == DialogResult.OK)
            {
                loadEntry(fd.FileName);
                UserSettings.G.PutEntry("openDocPath", Path.GetDirectoryName(fd.FileName));
            }
        }

        private void event_saveDoc(object sender, EventArgs e)
        {
            if (m_entry.GetEntry("filename").Length == 0)
            {
                event_saveAsDoc(sender, e);
            }
            else
            {
                saveEntry();
            }
        }

        private void event_saveAsDoc(object sender, EventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog ();

            string filename = m_entry.GetEntry("filename");

            if (filename.Length > 0)
            {
                fd.FileName = m_entry.GetEntry("filename");
                fd.InitialDirectory = Path.GetDirectoryName(fd.FileName);
            }
            else
            {
                fd.InitialDirectory = UserSettings.G.GetEntry("openDocPath");
            }

            fd.DefaultExt = ".tan";
            fd.Filter     = "Tanjunka files (*.tan)|*.tan|All files (*.*)|*.*";
            fd.RestoreDirectory = true;
            fd.Title      = "Save As...";

            if (fd.ShowDialog() == DialogResult.OK)
            {
                m_entry.PutEntry("filename", fd.FileName);
                saveEntry();
                UserSettings.G.PutEntry("openDocPath", Path.GetDirectoryName(fd.FileName));
            }
        }

        private void event_getPost(object sender, EventArgs e)
        {
            if (verifyIfModified(Localize.G.GetText("PromptGetEntry")))
            {
                if (UserSettings.G.GetNumPostSets() == 0)
                {
                    MessageBox.Show(Localize.G.GetText("ErrorNoPostSetsDefined"), Localize.G.GetText("LabelOops"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    PostsForm form = new PostsForm(m_curPS);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        // this is only because there is an OK button on the form
                        // so it's possible we didn't read an entry
                        if (form.GetEntry() != null)
                        {
                            m_curPS = form.GetPostSet();
                            UserSettings.G.OpOnPostSets(new turnOffAllPostSets());
                            m_curPS.SetOn(true);
                            UpdatePostSets();
                            m_entry = form.GetEntry();
                            m_entry.PutEntryNoMod("__published__", "true");
                            updateForms();
                        }
                    }
                    form.Dispose();
                }
            }
        }

        private void event_exit(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BodyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (verifyIfModified(Localize.G.GetText("PromptQuit")))
            {
                // save to temp file
                UserSettings.Save();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private string TrimDefault(string str, string def)
        {
            if (str.Length >= def.Length && String.Compare(str.Substring(0, def.Length), def) == 0)
            {
                return str.Substring(def.Length);
            }
            else
            {
                return str;
            }
        }

        private string InsertDefault(string str, string def)
        {
            return str.Length > 0 ? str : def;
        }

        // copy data from body page to entry
        private void update_fromBody(EntryInfo entry)
        {
            entry.PutEntry("postbody", TrimDefault(this.wbBody.GetInnerHTML("postbody"), Localize.G.GetText("PromptHtml")));
            entry.PutEntry("posttitle", TrimDefault(this.wbBody.GetInnerText("posttitle"), Localize.G.GetText("PromptTitle")));
        }

        // update Body page with current extra info
        private void update_bodyPage(EntryInfo entry)
        {
            string formFile = "tan-bodyform.html";
            m_allowBodyUpdate = true;
            if (m_curPS != null)
            {
                if (m_curPS.GetBlogService().GetBoolEntry("useforms"))
                {
                    string customFile = m_curPS.GetBlogService().GetEntry("bodyform");
                    if (File.Exists(customFile))
                    {
                        formFile = customFile;
                    }
                }
            }
            Util.SetFormHTML(this.wbBody, formFile);
        }

        private void body_docCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (m_curPS != null)
            {
                if (m_curPS.GetBlogService().GetBoolEntry("usestylesheet"))
                {
                    string cssFile = m_curPS.GetBlogService().GetEntry("stylesheet");
                    if (File.Exists(cssFile))
                    {
                        this.wbBody.SetCSSForm(cssFile);
                    }
                }
            }
            this.wbBody.SetInnerHTML("postbody", InsertDefault(m_entry.GetEntry("postbody"), Localize.G.GetText("PromptHtml")));
            this.wbBody.SetInnerText("posttitle", InsertDefault(m_entry.GetEntry("posttitle"), Localize.G.GetText("PromptTitle")));

            Splash.CloseForm();
        }

        private void extra_docCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (m_curPS != null)
            {
                if (m_curPS.GetBlogService().GetBoolEntry("usestylesheet"))
                {
                    string cssFile = m_curPS.GetBlogService().GetEntry("stylesheet");
                    if (File.Exists(cssFile))
                    {
                        this.wbExtra.SetCSSForm(m_curPS.GetBlogService().GetEntry("stylesheet"));
                    }
                }
            }
            this.wbExtra.SetInnerHTML("postbody", InsertDefault(m_entry.GetEntry("postextra"), Localize.G.GetText("PromptHtml")));
        }

        private void options_docCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (m_curPS != null)
            {
                m_curPS.GetBlogService().updateEntryToOptions(m_curPS, m_entry, this.wbOptions);
            }
        }

        private void execCommand(TanWebBrowser wb, int type, string v1, string v2, bool flag)
        {
            switch (type)
            {
                case 0:
                    {
                        Object[] args = new Object[4] { "", v1, v2, flag };
                        wb.Document.InvokeScript("tanExecCommand", args);
                    }
                    break;
                case 1:
                    {
                        Object[] args = new Object[] {v1, };
                        wb.Document.InvokeScript("setFocus", args);
                    }
                    break;
            }
        }
        // exec commmand for Body
        private void execCommand_body(int type, string v1, string v2, bool flag)
        {
            execCommand(this.wbBody, type, v1, v2, flag);
        }

        private void UICommand_body(TanTabPage.UICommandID cmd)
        {
            switch (cmd)
            {
            case TanTabPage.UICommandID.SpellCheckStart:
                textBeingSpellChecked = new StringBuilder(m_entry.GetEntry("postbody"));
                break;
            case TanTabPage.UICommandID.SpellCheckEnd:
                m_entry.PutEntry("postbody", textBeingSpellChecked.ToString());
                break;
            default:
                UICommand_body_extra(cmd);
                break;
            }
        }

        private void UICommand_extra(TanTabPage.UICommandID cmd)
        {
            switch (cmd)
            {
            case TanTabPage.UICommandID.SpellCheckStart:
                textBeingSpellChecked = new StringBuilder(m_entry.GetEntry("postextra"));
                break;
            case TanTabPage.UICommandID.SpellCheckEnd:
                m_entry.PutEntry("postextra", textBeingSpellChecked.ToString());
                break;
            default:
                UICommand_body_extra(cmd);
                break;
            }
        }

        private void UICommand_body_extra(TanTabPage.UICommandID cmd)
        {
            switch (cmd)
            {
            case TanTabPage.UICommandID.Undo:
                m_curTab.ExecCommand(0,"Undo", null);
                break;
            case TanTabPage.UICommandID.Redo:
                m_curTab.ExecCommand(0,"Redo", null);
                break;
            }
        }

        private string queryCommandValue_body(string cmd)
        {
            Object[] args = new Object[2] { "", cmd };
            Object ob = this.wbBody.Document.InvokeScript("tanQueryCommandValue", args);
            return ob.ToString();
        }
        private void insertHTML_body(string value)
        {
            Object[] args = new Object[2] { "postbody", value };
            wbBody.Document.InvokeScript("insertHTML", args);
        }
        private void insertHTML_extra(string value)
        {
            Object[] args = new Object[2] { "postbody", value };
            wbExtra.Document.InvokeScript("insertHTML", args);
        }
        // copy data from extra page to entry
        private void update_fromExtra(EntryInfo entry)
        {
            entry.PutEntry("postextra", TrimDefault(this.wbExtra.GetInnerHTML("postbody"), Localize.G.GetText("PromptHtml")));
        }
        // update Extra page with current extra info
        private void update_extraPage(EntryInfo entry)
        {
            m_allowExtraUpdate = true;
            string formFile = "tan-moreform.html";
            m_allowBodyUpdate = true;
            if (m_curPS != null)
            {
                if (m_curPS.GetBlogService().GetBoolEntry("useforms"))
                {
                    string customFile = m_curPS.GetBlogService().GetEntry("moreform");
                    if (File.Exists(customFile))
                    {
                        formFile = customFile;
                    }

                }
            }
            Util.SetFormHTML(this.wbExtra, formFile);
        }
        // exec commmand for extra
        private void execCommand_extra(int type, string v1, string v2, bool flag)
        {
            execCommand(this.wbExtra, type, v1, v2, flag);
        }

        private string queryCommandValue_extra(string cmd)
        {
            Object[] args = new Object[2] { "", cmd };
            Object ob = this.wbExtra.Document.InvokeScript("tanQueryCommandValue", args);
            return ob.ToString();
        }
        // copy data from bodyHTML page to entry
        private void update_fromBodyHTML(EntryInfo entry)
        {
            entry.PutEntry("postbody", this.textBoxBodyHTML.Text);
        }
        // update bodyHTML page with current extra info
        private void update_bodyHTMLPage(EntryInfo entry)
        {
            this.textBoxBodyHTML.Text = entry.GetEntry("postbody");
        }
        // copy data from extraHTML page to entry
        private void update_fromExtraHTML(EntryInfo entry)
        {
            entry.PutEntry("postextra", this.textBoxExtraHTML.Text);
        }
        // update extraHTML page with current entry info
        private void update_extraHTMLPage(EntryInfo entry)
        {
            this.textBoxExtraHTML.Text = entry.GetEntry("postextra");
        }

        // update options page with current entry info
        private void update_optionsPage(EntryInfo entry)
        {
            m_allowOptionsUpdate = true;
            Util.SetFormHTML(this.wbOptions, (m_curPS != null) ? m_curPS.GetBlogService().GetOptionForm() : "tan-nooptions.html");

//            if (m_curPS != null)
//            {
//                m_curPS.GetBlogService().updateEntryToOptions(m_curPS, entry, this.wbOptions);
//            }
        }
        // copy data from options page to entry
        private void update_fromOptions(EntryInfo entry)
        {
            if (m_curPS != null)
            {
                m_curPS.GetBlogService().updateOptionsToEntry(m_curPS, entry, this.wbOptions);
            }
        }

        private void tabControl1_GotFocus(object sender, EventArgs e)
        {
            if (this.tabMainTabs.TabCount > 0)
            {
                TanTabPage newTab = this.tabMainTabs.SelectedTab as TanTabPage;
                newTab.Focus();
           }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TanTabPage newTab = (TanTabPage)this.tabMainTabs.SelectedTab;

            if (m_curTab != null)
            {
                if (this.tabMainTabs.Visible)
                {
                    m_curTab.UpdateEntry(m_entry);
                }
            }
            if (newTab != null)
            {
                m_curTab = newTab;
                if (this.tabMainTabs.Visible)
                {
                    m_curTab.UpdatePage(m_entry);
                }
            }
        }

        // ---------------------------------------------------------------
        //
        // toolbar buttons
        //
        private void cutToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0,"Cut", "");
        }
        private void copyToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0,"Copy", "");
        }
        private void pasteToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0,"Paste", null);
        }
        private void undoToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.UICommand(TanTabPage.UICommandID.Undo);
        }
        private void redoToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.UICommand(TanTabPage.UICommandID.Redo);
        }
        private void selectAllToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0,"SelectAll", null);
        }
        private void postSet_selectedIndexChanged(object sender, EventArgs e)
        {
            string dummy = "";

            if (!m_supressEvents)
            {
                Object ob = postSetToolStripComboBox.SelectedItem;
                if (ob.GetType() != dummy.GetType())
                {
                    syncEntryFromForm();
                    m_curPS = (PostSet)ob;
                    updateForms();
                }
            }
        }
        private void style_selectedIndexChanged(object sender, EventArgs e)
        {
            if (!m_supressEvents)
            {
                StyleInfo si = (StyleInfo) styleToolStripComboBox.SelectedItem;
                m_curTab.ExecCommand(0,"FormatBlock", si.tag);
            }
        }
        private void fontSize_selectedIndexChanged(object sender, EventArgs e)
        {
            if (!m_supressEvents)
            {
                string fSize = (string) fontSizeToolStripComboBox.SelectedItem;
                m_curTab.ExecCommand(0,"FontSize", fSize);
            }
        }
        private void linkToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0,"CreateLink", "", true);
        }

        private void insertImages()
        {
            OpenFileDialog fd = new OpenFileDialog ();
            string foldername = UserSettings.G.GetEntry("insertImagePath");
            fd.InitialDirectory = foldername;

            fd.DefaultExt  = ".jpg";
            fd.Filter      = "Image files (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif|All files (*.*)|*.*";
            fd.RestoreDirectory = true;
            fd.Title       = "Insert Image(s)...";
            fd.Multiselect = true;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                string[] files = fd.FileNames;
                StringBuilder sb = new StringBuilder();

                foreach (string file in files)
                {
                    Log.Printn (file);
                    sb.Append(AddFile (file));

                    UserSettings.G.PutEntry("insertImagePath", Path.GetDirectoryName(file));
                }

                m_curTab.InsertHTML(sb.ToString());
            }
        }
        private void insertImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            insertImages();
        }
        private void imageToolStripButton_Click(object sender, EventArgs e)
        {
            insertImages();
        }
        private void boldToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "Bold", "");
        }
        private void italicToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "Italic", "");
        }
        private void underlineToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "Underline", "");
        }
        private void leftJustifyToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "JustifyLeft", "");
        }
        private void centerJustifyToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "JustifyCenter", "");
        }
        private void rightJustifyToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "JustifyRight", "");
        }
        private void justifyToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "JustifyFull", "");
        }
        private void orderedListToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "InsertOrderedList", "");
        }
        private void unorderedListToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "InsertUnorderedList", "");
        }
        private void indentToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "Indent", "");
        }
        private void outdentToolStripButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "Outdent", "");
        }
        private void highlightSet_Click(object sender, EventArgs e)
        {
            TanColorMenuItem tcmi = (TanColorMenuItem)sender;

            // set default highlight color
            this.highlightToolStripSplitButton.TanColor   = tcmi.TanColor;
            this.highlightToolStripSplitButton.TanColorID = tcmi.TanColorID;
            this.highlightToolStripSplitButton.Invalidate();
            m_curTab.ExecCommand(0, "BackColor", this.highlightToolStripSplitButton.TanColorID);
        }
        private void highlightToolStripSplitButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "BackColor", this.highlightToolStripSplitButton.TanColorID);
        }
        private void fontColorSet_Click(object sender, EventArgs e)
        {
            TanColorMenuItem tcmi = (TanColorMenuItem)sender;

            // set default fontColor color
            this.fontColorToolStripSplitButton.TanColor   = tcmi.TanColor;
            this.fontColorToolStripSplitButton.TanColorID = tcmi.TanColorID;
            this.fontColorToolStripSplitButton.Invalidate();
            m_curTab.ExecCommand(0, "ForeColor", this.fontColorToolStripSplitButton.TanColorID);
        }
        private void fontColorToolStripSplitButton_Click(object sender, EventArgs e)
        {
            m_curTab.ExecCommand(0, "ForeColor", this.fontColorToolStripSplitButton.TanColorID);
        }

        private void MassageClipboard()
        {
            IDataObject iData = Clipboard.GetDataObject();
            StringBuilder sb = new StringBuilder();

            if (iData.GetDataPresent(DataFormats.Bitmap))
            {
                Log.Printn ("--paste bitmap--");
                Bitmap bm = new Bitmap((Image)iData.GetData(DataFormats.Bitmap));
                string tfile = TJPagePicture.CreateTempBitmap(bm);
                sb.Append(AddFile(tfile));
            }
            else if (iData.GetDataPresent(DataFormats.Dib))
            {
                Log.Printn ("--paste DIB (not implemented)--");
            }
            else if (iData.GetDataPresent(DataFormats.FileDrop))
            {
                Log.Printn ("--paste fileDrop--");
                string[] files = (string[])iData.GetData(DataFormats.FileDrop);

                foreach (string file in files)
                {
                    Log.Printn (file);
                    sb.Append(AddFile (file));
                }
            }
            else if (iData.GetDataPresent(DataFormats.Html))
            {
                Log.Printn ("--paste html--");
                string html = (string)iData.GetData(DataFormats.Html);

                if (UserSettings.G.GetBoolEntry("prefs_copylocally"))
                {
                    int startFrag;
                    int endFrag;
                    int startHtml;
                    int endHtml;
                    string srcURL = "";

                    if (GetHTMLClipboardIntVar(html, "StartHTML", out startHtml) &&
                        GetHTMLClipboardIntVar(html, "EndHTML", out endHtml) &&
                        GetHTMLClipboardIntVar(html, "StartFragment", out startFrag) &&
                        GetHTMLClipboardIntVar(html, "EndFragment", out endFrag) &&
                        GetHTMLClipboardStrVar(html, "SourceURL", out srcURL)
                        )
                    {
                        string frag = html.Substring(startFrag, endFrag - startFrag);

                        // make all relative links absolute
                        {
                            RelLinkToAbs rlta = new RelLinkToAbs();

                            frag = rlta.ConvertRelativeLinksToAbsolute(srcURL, frag);
                        }

                        // find all <img> tags and copy the files locally
                        // should I only check img? maybe I should check url() and background=""
                        {
                            Regex r = Util.MakeRegex("(?<1><img\\s+)(?<2>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[^>\"'/])*)(?<3>/*>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                            frag = r.Replace(frag, new MatchEvaluator(this.CopyImagesLocally));
                        }

                        string start = html.Substring(startHtml, startFrag - startHtml);
                        string end = "";

                        // some how the damn clipboard can reaturn an endHtml that is PAST THE END OF THE STRING!!
                        int endStartPos = endFrag;
                        if (endFrag < html.Length)
                        {
                            if (endHtml >= html.Length)
                            {
                                endHtml = html.Length;
                            }
                            end = html.Substring(endFrag, endHtml - endFrag);
                        }

                        string head  =
                            "Version:1.0\r\n" +
                            "StartHTML:000000000\r\n" +
                            "EndHTML:000000000\r\n" +
                            "StartFragment:000000000\r\n" +
                            "EndFragment:000000000\r\n" +
                            "StartSelection:000000000\r\n" +
                            "EndSelection:000000000\r\n" +
                            "SourceURL:" + srcURL + "\r\n";
                        head = String.Format(
                            "Version:1.0\r\n" +
                            "StartHTML:{0:d9}\r\n" +
                            "EndHTML:{1:d9}\r\n" +
                            "StartFragment:{2:d9}\r\n" +
                            "EndFragment:{3:d9}\r\n" +
                            "StartSelection:{2:d9}\r\n" +
                            "EndSelection:{3:d9}\r\n" +
                            "SourceURL:" + srcURL + "\r\n"
                            , head.Length
                            , head.Length + start.Length + frag.Length + end.Length
                            , head.Length + start.Length
                            , head.Length + start.Length + frag.Length);


                        html = head + start + frag + end;
                    }
                }

                sb.Append(html);
            }
            else if (iData.GetDataPresent(DataFormats.SymbolicLink))
            {
                Log.Printn ("--paste symbolic link--");
            }
            else if (iData.GetDataPresent(DataFormats.UnicodeText))
            {
                Log.Printn ("--paste unicodetext--");
            }
            else if (iData.GetDataPresent(DataFormats.Text))
            {
                Log.Printn ("--paste text--");
            }

            // only update clibboard if we changed something
            if (sb.Length > 0)
            {
                DataObject myData = new DataObject();
                myData.SetData(DataFormats.Html, true, sb.ToString());
                Clipboard.SetDataObject(myData, true);
            }
        }

        private bool GetHTMLClipboardIntVar(string html, string id, out int var)
        {
            string val;
            var = 0;
            if (GetHTMLClipboardStrVar(html, id, out val))
            {
                var = int.Parse(val);
                return true;
            }
            return false;
        }
        private bool GetHTMLClipboardStrVar(string html, string id, out string var)
        {
            var = "";
            Regex r = Util.MakeRegex(id + ":(?<1>.*?)\\r", RegexOptions.IgnoreCase|RegexOptions.Compiled);
            Match m = r.Match(html);

            if (m.Groups.Count > 1)
            {
                var = m.Groups[1].ToString();
                return true;
            }
            return false;
        }

        public class RelLinkToAbs
        {
            private string srcURL;

            public RelLinkToAbs() { }

            public string MakeRelLinkAbsolute (Match m)
            {
                string src = Util.RemoveQuotes(m.Groups[2].ToString());

                if (src.Length > 10)
                {
                    // see if there is a "://" or ":\\" in the first 10 chars
                    Regex r = Util.MakeRegex("(?://|:\\\\)", RegexOptions.IgnoreCase|RegexOptions.Compiled);
                    Match mf = r.Match(src.Substring(0,10));
                    if (!mf.Success)
                    {
                        src = Util.MergeURLPaths(srcURL, src);
                    }
                }

                return m.Groups[1].ToString() + "=\"" + src + "\"";
            }

            public string ConvertRelativeLinksToAbsolute(string url, string html)
            {
                // find the "src"
                srcURL = url;

                Regex r = Util.MakeRegex("(?<1>(?:src|href))\\s*=\\s*(?<2>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*))", RegexOptions.IgnoreCase|RegexOptions.Compiled);
                return r.Replace(html, new MatchEvaluator(this.MakeRelLinkAbsolute));
            }
        }

        private string CopyImagesLocally(Match m)
        {
            string middle = m.Groups[2].ToString();

            // find the "src"
            Regex r1 = Util.MakeRegex("src\\s*=\\s*(?<1>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*))", RegexOptions.IgnoreCase|RegexOptions.Compiled);
            Match srcMatch = r1.Match(middle);
            if (srcMatch.Groups.Count > 1)
            {
                // extract the URL
                string src = Util.RemoveQuotes(srcMatch.Groups[1].ToString());
                int width  = 0;
                int height = 0;

                // extract the displayed width and height (since the actual image might be larger)
                Regex r2 = Util.MakeRegex("width\\s*=\\s*(?<1>\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Match mw = r2.Match(middle);
                if (mw.Groups.Count > 1)
                {
                    width = Util.SafeIntParse(Util.RemoveQuotes(mw.Groups[1].ToString()));
                }
                Regex r3 = Util.MakeRegex("height\\s*=\\s*(?<1>\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*)", RegexOptions.IgnoreCase|RegexOptions.Compiled);
                Match mh = r3.Match(middle);
                if (mh.Groups.Count > 1)
                {
                    height = Util.SafeIntParse(Util.RemoveQuotes(mh.Groups[1].ToString()));
                }

                // which kinds can we handle?
                string ext = Path.GetExtension(src).ToLower();
                if (TJPagePicture.CanHandle(src))
                {
                    try
                    {
                        TJPagePicture tpic;

                        if (m_entry.PictureTempExists(src))
                        {
                            // get old pic
                            tpic = m_entry.GetTempPicture(src);
                        }
                        else
                        {
                            // make a new pic
                            string newFilename = Util.getTempFilename(ext);
                            WebClient client = new WebClient ();
                            client.DownloadFile(src, newFilename);
                            client.Dispose();

                            tpic = m_entry.GetNewPicture(newFilename);
                            if (width > 0 && height > 0)
                            {
                                m_entry.resizeTempFile(tpic, width, height);
                            }
                        }

                        middle = r1.Replace(middle, "src=\"" + tpic.GetTempPath() + "\"");
                        middle = r2.Replace(middle, "width=\"" + tpic.GetNewWidth() + "\"");
                        middle = r3.Replace(middle, "height=\"" + tpic.GetNewHeight() + "\"");

                    }
                    catch (Exception ex)
                    {
                        Log.Printn("could not download (" + src + ")\n" + ex.Message);
                    }
                }
            }

            return m.Groups[1].ToString() + middle + "/>";
        }

#if false
        private void PickDate()
        {
            CalendarForm form = new CalendarForm();

            if (form.ShowDialog() == DialogResult.OK)
            {
            }
            form.Dispose();
        }
#endif

        private void OptionsFormCommand (int value)
        {
            if (m_curPS != null)
            {
                m_curPS.GetBlogService().OptionsFormCommand(m_entry, this.wbOptions, value);
            }
        }

        private void FormCommand (int value1)
        {
            switch (value1)
            {
            case 1:
                MassageClipboard();
                break;
            case 2:
//                PictureContextMenu()
                break;
            case 3:
//                 PickDate();
                 break;
            }
        }

        private string ReturnOriginalImageSizeToJScript (string imagePath)
        {
            if (m_entry.PictureTempExists(imagePath))
            {
                TJPagePicture tpic = m_entry.GetTempPicture(imagePath);

                return String.Format("{0:d9}:{1:d9}", tpic.GetOrigWidth(), tpic.GetOrigHeight());
            }
            return "";
        }

        private string ReturnImage8bit (string imagePath)
        {
            if (m_entry.PictureTempExists(imagePath))
            {
                TJPagePicture tpic = m_entry.GetTempPicture(imagePath);

                return tpic.Is8Bit() ? "1" : "0";
            }
            return "0";
        }

        private string JScriptInfoRequest(int cmd, string str1, string str2)
        {
            switch (cmd)
            {
            case 1:
                return ReturnOriginalImageSizeToJScript(str1);
            case 2:
                return ReturnImage8bit(str1);
            }
            return "";
        }

        private string ComplexCallback (int cmd, string str, int v1, int v2)
        {
            switch (cmd)
            {
            case 1:
                return RescaleImage(str, v1, v2);
            case 2:
                return ConvertToJPEG(str);
            case 3:
                EditImage(str);
                return "";
            }
            return "";
        }

        private void EditImage(string imagePath)
        {
            if (m_entry.PictureTempExists(imagePath))
            {
                TJPagePicture tpic = m_entry.GetTempPicture(imagePath);

                SetEditImage(tpic);
                this.tabMainTabs.SelectedTab = this.tabImage;
            }
        }

        private string ConvertToJPEG(string imagePath)
        {
            if (m_entry.PictureTempExists(imagePath))
            {
                TJPagePicture tpic = m_entry.GetTempPicture(imagePath);

                tpic.BumpVersion(); // so it gets re-uploaded if it has already been uploaded
                m_entry.ConvertToJPEG(tpic);

                return tpic.GetTempPath();
            }

            return "";
        }

        private string RescaleImage(string imagePath, int newWidth, int newHeight)
        {
            if (m_entry.PictureTempExists(imagePath))
            {
                TJPagePicture tpic = m_entry.GetTempPicture(imagePath);

                tpic.BumpVersion(); // so it gets re-uploaded if it has already been uploaded
                m_entry.resizeTempFile(tpic, newWidth, newHeight);

                return tpic.GetTempPath();
            }

            return "";
        }

        private void wbBody_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void tabControl1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Dib, false) == true)
            {
                Log.Printn ("--dropped DIB--");

                if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    // loop through the string array, adding each filename to the ListBox

                    foreach (string file in files)
                    {
                        Log.Printn (file);
                    }
                }
                else
                {
                    Log.Printn ("*no filedrop info for dib");
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                // get the filenames
                // (yes, everything to the left of the "=" can be put in the
                // foreach loop in place of "files", but this is easier to understand.)

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped file--");
                foreach (string file in files)
                {
                    Log.Printn (file);
                }
            }
            else if (e.Data.GetDataPresent("UniformResourceLocator", false) == true)
            {
                Log.Printn ("--dropped URL--");
                {
                    string link = (string)e.Data.GetData(DataFormats.UnicodeText);

                    Log.Printn ("part1:" + link);
                }
                {
                    System.IO.Stream ioStream =
                    (System.IO.Stream)e.Data.GetData("FileGroupDescriptor");
                    byte[] contents = new Byte[2048];
                    ioStream.Read(contents, 0, 2048);
                    ioStream.Close();
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    //The magic number 76 is the size of that part of the
                    //FILEGROUPDESCRIPTOR structure before
                    // the filename starts - cribbed
                    //from another usenet post.
                    for (int i = 76; contents[i] != 0; i++)
                    {
                        sb.Append((char)contents[i]);
                    }
                    if (!sb.ToString(sb.Length - 4, 4).ToLower().Equals(".url"))
                    {
                        throw new Exception("filename does not end in '.url'");
                    }
                    string link = sb.ToString(0, sb.Length - 4);

                    Log.Printn ("part2:" + link);
                }

            }
            else if (e.Data.GetDataPresent(DataFormats.SymbolicLink, false) == true)
            {
                string link = (string)e.Data.GetData(DataFormats.SymbolicLink);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped link--");
                Log.Printn (link);
            }
            else if (e.Data.GetDataPresent(DataFormats.Html, false) == true)
            {
                string link = (string)e.Data.GetData(DataFormats.Html);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped html--");
                Log.Printn (link);
            }
            else if (e.Data.GetDataPresent(DataFormats.Rtf, false) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.Rtf);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped rtf--");
                foreach (string file in files)
                {
                    Log.Printn (file);
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.UnicodeText, false) == true)
            {
                string unitext = (string)e.Data.GetData(DataFormats.UnicodeText);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped unicode--");
                Log.Printn (unitext);
            }
            else if (e.Data.GetDataPresent(DataFormats.Text, true) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.Text);

                // loop through the string array, adding each filename to the ListBox

                Log.Printn ("--dropped text--");
                foreach (string file in files)
                {
                    Log.Printn (file);
                }
            }
        }

        private void tabControl1_DragEnter(object sender, DragEventArgs e)
        {
             // make sure they're actually dropping files (not text or anything else)
            {
                string[] formats = e.Data.GetFormats();

                Log.Printn ("---formats---");
                foreach (string format in formats)
                {
                    Log.Printn (format);
                }
            }

            if (
                (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Bitmap, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Dib, false) == true) ||
                (e.Data.GetDataPresent("UniformResourceLocator", false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Html, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Rtf, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.UnicodeText, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.SymbolicLink, false) == true) ||
                (e.Data.GetDataPresent(DataFormats.Text, true) == true) ||
                false
                )
            {
                // allow them to continue
                // (without this, the cursor stays a "NO" symbol
                e.Effect = DragDropEffects.All;
            }
        }

        private void postSetDropDown_Handler(object sender, System.EventArgs e)
        {
            ToolStripComboBox senderComboBox = sender as ToolStripComboBox;
#if false
            senderComboBox.DropDownWidth = 300;
#else
            int width = senderComboBox.DropDownWidth;
            Graphics g = senderComboBox.ComboBox.CreateGraphics();
            Font font = senderComboBox.Font;
            int vertScrollBarWidth =
                (senderComboBox.Items.Count>senderComboBox.MaxDropDownItems)
                ?SystemInformation.VerticalScrollBarWidth:0;

            int newWidth;
            foreach (object ob in senderComboBox.Items)
            {
                string s = ob.ToString();
                newWidth = (int) g.MeasureString(s, font).Width
                    + vertScrollBarWidth;
                if (width < newWidth )
                {
                    width = newWidth;
                }
            }
            g.Dispose();
            senderComboBox.DropDownWidth = width;
#endif
        }
        // ***************************** spellchecking *****************************

        private void spelling_DeletedWord(object sender, NetSpell.SpellChecker.SpellingEventArgs e)
        {
            textBeingSpellChecked.Remove(e.TextIndex, e.Word.Length);
        }

        private void spelling_ReplacedWord(object sender, NetSpell.SpellChecker.ReplaceWordEventArgs e)
        {
            textBeingSpellChecked.Remove(e.TextIndex, e.Word.Length);
            textBeingSpellChecked.Insert(e.TextIndex, e.ReplacementWord);
        }

        private void spelling_EndOfText(object sender, System.EventArgs e)
        {
            //
            //MessageBox.Show("end of text", "yeah", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void spelling_Closed(object sender, EventArgs e)
        {
            System.Windows.Forms.Form form = sender as Form;
            if (!form.Visible)
            {
                this.Enabled = true;
                m_curTab.UICommand(TanTabPage.UICommandID.SpellCheckEnd);
                syncFormFromEntry();
            }
        }

        private void UpdateUILanguage()
        {
            this.SuspendLayout();
            this.Visible = false;

            this.newpostButton.Text = Localize.G.GetText("MainNewPostButton");
            this.republishButton.Text = Localize.G.GetText("MainRepostButton");
            this.publishButton.Text = Localize.G.GetText("MainPostButton");
            this.newToolStripButton.Text = Localize.G.GetText("ToolStripNew");
            this.openToolStripButton.Text = Localize.G.GetText("ToolStripOpen");
            this.saveToolStripButton.Text = Localize.G.GetText("ToolStripSave");
            this.cutToolStripButton.Text = Localize.G.GetText("ToolStripCut");
            this.copyToolStripButton.Text = Localize.G.GetText("ToolStripCopy");
            this.pasteToolStripButton.Text = Localize.G.GetText("ToolStripPaste");
            this.imageToolStripButton.Text = Localize.G.GetText("ToolStripImage");
            this.linkToolStripButton.Text = Localize.G.GetText("ToolStripLink");
            this.boldToolStripButton.Text = Localize.G.GetText("ToolStripBold");
            this.italicToolStripButton.Text = Localize.G.GetText("ToolStripItalics");
            this.underlineToolStripButton.Text = Localize.G.GetText("ToolStripUnderline");
            this.leftJustifyToolStripButton.Text = Localize.G.GetText("ToolStripLeft");
            this.centerJustifyToolStripButton.Text = Localize.G.GetText("ToolStripCenter");
            this.rightJustifyToolButton.Text = Localize.G.GetText("ToolStripRight");
            this.justifyToolStripButton.Text = Localize.G.GetText("ToolStripJustify");
            this.orderedListToolStripButton.Text = Localize.G.GetText("ToolStripOrderedList");
            this.unorderedListToolStripButton.Text = Localize.G.GetText("ToolStripUnorderedList");
            this.outdentToolStripButton.Text = Localize.G.GetText("ToolStripOutdent");
            this.indentToolStripButton.Text = Localize.G.GetText("ToolStripIndent");
            this.highlightToolStripSplitButton.Text = Localize.G.GetText("ToolStripHighlight");
            this.fontColorToolStripSplitButton.Text = Localize.G.GetText("ToolStripFontColor");
            this.fileToolStripMenuItem.Text = Localize.G.GetText("MenuFile");
            this.newToolStripMenuItem.Text = Localize.G.GetText("MenuFileNew");
            this.openToolStripMenuItem.Text = Localize.G.GetText("MenuFileOpen");
            this.saveToolStripMenuItem.Text = Localize.G.GetText("MenuFileSave");
            this.saveAsToolStripMenuItem.Text = Localize.G.GetText("MenuFileSaveAs");
            this.getPostToolStripMenuItem.Text = Localize.G.GetText("MenuFileGetEntry");
            this.exitToolStripMenuItem.Text = Localize.G.GetText("MenuFileExit");
            this.editToolStripMenuItem.Text = Localize.G.GetText("MenuEdit");
            this.undoToolStripMenuItem.Text = Localize.G.GetText("MenuEditUndo");
            this.redoToolStripMenuItem.Text = Localize.G.GetText("MenuEditRedo");
            this.cutToolStripMenuItem.Text = Localize.G.GetText("MenuEditCut");
            this.copyToolStripMenuItem.Text = Localize.G.GetText("MenuEditCopy");
            this.pasteToolStripMenuItem.Text = Localize.G.GetText("MenuEditPaste");
            this.selectAllToolStripMenuItem.Text = Localize.G.GetText("MenuEditSelectAll");
            this.insertToolStripMenuItem.Text = Localize.G.GetText("MenuInsert");
            this.insertImageToolStripMenuItem.Text = Localize.G.GetText("MenuInsertImage");
            this.postToolStripMenuItem2.Text = Localize.G.GetText("MenuPost");
            this.publistToolStripMenuItem.Text = Localize.G.GetText("MenuPostPost");
            this.postSetsToolStripMenuItem.Text = Localize.G.GetText("MenuPostPostSets");
            this.servicesToolStripMenuItem.Text = Localize.G.GetText("MenuPostServices");
            this.toolsToolStripMenuItem.Text = Localize.G.GetText("MenuTools");
            this.preferencesToolStripMenuItem.Text = Localize.G.GetText("MenuToolsPrefs");
            this.helpToolStripMenuItem.Text = Localize.G.GetText("MenuHelp");
            this.helpFileToolStripMenuItem.Text = Localize.G.GetText("MenuHelpHelp");
            this.aboutToolStripMenuItem.Text = Localize.G.GetText("MenuHelpAbout");
            this.webpageToolStripMenuItem.Text = Localize.G.GetText("MenuHelpWebpage");
            this.updateToolStripMenuItem.Text = Localize.G.GetText("MenuToolsUpdate");
            this.spellcheckToolStripMenuItem.Text = Localize.G.GetText("MenuToolsSpellCheck");
            this.forumToolStripMenuItem.Text = Localize.G.GetText("MenuHelpForum");
            this.debugToolStripMenuItem.Text = Localize.G.GetText("MenuDebug");
            this.reloadeditformToolStripMenuItem.Text = Localize.G.GetText("MenuDebugReload");
            this.showBlogInfoFormToolStripMenuItem.Text = Localize.G.GetText("MenuDebugShowInfo");
            this.findandReplaceToolStripMenuItem.Text = Localize.G.GetText("MenuEditFind");
            this.insertSymbolToolStripMenuItem.Text = Localize.G.GetText("MenuInsertSymbol");
            this.tabBody.Text = Localize.G.GetText("TabBody");
            this.tabExtra.Text = Localize.G.GetText("TabExtra");
            this.tabImage.Text = Localize.G.GetText("TabPhoto");
            this.paRevertButton.Text = Localize.G.GetText("PhotoRevert");
            this.paUndoButton.Text = Localize.G.GetText("PhotoUndo");
            this.paRedoButton.Text = Localize.G.GetText("PhotoRedo");
            this.tabCrop.Text = Localize.G.GetText("PhotoTabCrop");
            this.tabBright.Text = Localize.G.GetText("PhotoTabBright");
            this.label1.Text = Localize.G.GetText("PhotoContrast");
            this.label2.Text = Localize.G.GetText("PhotoBright");
            this.tabHSV.Text = Localize.G.GetText("PhotoTabHue");
            this.label3.Text = Localize.G.GetText("PhotoHue");
            this.label4.Text = Localize.G.GetText("PhotoSaturation");
            this.label5.Text = Localize.G.GetText("PhotoLightness");
            this.tabOptions.Text = Localize.G.GetText("TabOptions");
            this.tabBodyHTML.Text = Localize.G.GetText("TabBodyHTML");
            this.tabExtraHTML.Text = Localize.G.GetText("TabExtraHTML");
            this.tabLog.Text = Localize.G.GetText("TabLog");
            this.label6.Text = Localize.G.GetText("PhotoTintHue");
            this.label7.Text = Localize.G.GetText("PhotoTintAmount");
            this.label8.Text = Localize.G.GetText("PhotoGamma");
            this.label9.Text = Localize.G.GetText("PhotoRotate");
            this.label10.Text = Localize.G.GetText("PhotoCrop");
            this.cropButton.Text = Localize.G.GetText("PhotoCrop");
            this.resetCropButton.Text = Localize.G.GetText("PhotoReset");

            this.Visible = true;
            this.ResumeLayout(false);
        }
    }
}


