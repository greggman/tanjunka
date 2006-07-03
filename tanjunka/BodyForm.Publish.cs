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
        public void publish_for_real()
        {
            if (UserSettings.G.GetNumPostSets() == 0)
            {
                MessageBox.Show(Localize.G.GetText("ErrorNoPostSetsDefined"), Localize.G.GetText("LabelOops"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (UserSettings.G.GetNumOnPostSets() == 0)
            {
                MessageBox.Show(Localize.G.GetText("ErrorNoPostSetsSelected"), Localize.G.GetText("LabelOops"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.Enabled = false;
            Cursor = Cursors.WaitCursor;
            m_psf = new PublishStatusForm(bgw);
            m_psf.Text = "Publishing...";

            int postCount = m_entry.GetIntEntry("postCount");
            if (postCount == 0)
            {
                m_entry.PutIntEntry("postCount", 1);
            }
            syncEntryFromForm();
            AddImagesInHTMLToUpload();
            Log.SetBackgroundWorker(bgw);
            this.bgw.RunWorkerAsync();
            m_psf.Show();
        }

        class uploadImageOp : ArrayListOperator
        {
            EntryInfo           m_entry;
            BlogPhotoService    m_bps;
            public int          m_itemNumber;
            TanBackgroundWorker m_bgw;

            public uploadImageOp (EntryInfo entry, TanBackgroundWorker bgw, BlogPhotoService bps, int itemNum)
            {
                m_entry = entry;
                m_bgw = bgw;
                m_bps = bps;
                m_itemNumber = itemNum;
            }

            public override bool operation (Object ob)
            {
                TJPagePicture tjpic = (TJPagePicture)ob;

                bs_refresh();

                // is this picture uploaded to this service?
                // we know this if the version number for this picture
                // in the serivce is the same as the global version number for the pic
                int serviceVersionNumber = tjpic.GetIntPhotoEntry(m_bps, BlogPhotoService.PSID_PIC_VERSION);
                if (serviceVersionNumber != tjpic.GetVersion())
                {
                    m_bgw.UpdateMessage(String.Format("Uploading: {0}", Path.GetFileName(tjpic.GetOriginalPath())));
                    m_bgw.UpdateProgress(m_itemNumber);
                    bs_refresh();

                    // upload the picture to this service
                    try
                    {
                        m_bps.PostPhoto(m_entry, tjpic);
                    }
                    catch (Exception ex)
                    {
                        string err = String.Format(Localize.G.GetText("ErrorCantPostImage"), Path.GetFileName(tjpic.GetOriginalPath()), m_bps.GetName());
                        throw new ApplicationException(err + "\n\n" + ex.Message);
                    }

                    bs_refresh();
                }

                if (m_bgw.CancellationPending)
                {
                    m_bgw.e.Cancel = true;
                    return false;
                }

                m_itemNumber++;

                return true;
            }
        }

        class postToServices : ArrayListOperator
        {
            EntryInfo           m_entry;
            TanBackgroundWorker m_bgw;
            BlogPhotoService    m_curPhotoService;
            int                 m_itemNumber;

            public postToServices(EntryInfo entry, TanBackgroundWorker bgw)
            {
                m_entry = entry;
                m_bgw = bgw;
                m_itemNumber = 0;
            }

            private string ReplaceSRCWidthHeight(Match m)
            // Replace each Regex cc match with the number of the occurrence.
            {
                #if false
                    Log.Printn("-----------------");
                    Log.Printn("groups(" + m.Groups.Count.ToString() + ")");
                    for (int ii = 0; ii < m.Groups.Count; ++ii)
                    {
                        Log.Printn("--part" + ii.ToString() + "--");
                        Log.Printn(m.Groups[ii].ToString());
                    }
                #endif

                Log.Printn("--part1--");
                Log.Printn(m.Groups[1].ToString());
                Log.Printn("--part2--");
                Log.Printn(m.Groups[2].ToString());

                string middle = m.Groups[2].ToString();

                // find the "src"
                Regex r = Util.MakeRegex("src\\s*=\\s*(?<1>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*))", RegexOptions.IgnoreCase|RegexOptions.Compiled);

                Match srcMatch = r.Match(middle);
                if (srcMatch.Groups.Count > 1)
                {
                    string src = Util.RemoveQuotes(srcMatch.Groups[1].ToString());

                    if (m_entry.PictureTempExists(src))
                    {
                        if (m_curPhotoService == null)
                        {
                            return "";
                        }
                        else
                        {
                            TJPagePicture tjpic = m_entry.GetTempPicture(src);
                            string newSrc = tjpic.GetPhotoEntry(m_curPhotoService, BlogPhotoService.PSID_PIC_URL);
                            string newWidth = tjpic.GetPhotoEntry(m_curPhotoService, BlogPhotoService.PSID_PIC_WIDTH);
                            string newHeight = tjpic.GetPhotoEntry(m_curPhotoService, BlogPhotoService.PSID_PIC_HEIGHT);

                            Regex r1 = Util.MakeRegex("src\\s*=\\s*(?<1>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                            middle = r1.Replace(middle, "src=\"" + newSrc + "\"");
                            if (newWidth.Length > 0 && newHeight.Length > 0)
                            {
                                Regex r2 = Util.MakeRegex("width\\s*=\\s*(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                                middle = r2.Replace(middle, "width=\"" + newWidth + "\"");
                                Regex r3 = Util.MakeRegex("height\\s*=\\s*(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                                middle = r3.Replace(middle, "height=\"" + newHeight + "\"");
                            }
                            else
                            {
                                // there is no guarnteed with in this particular photoservice so remove the width/height
                                Regex r2 = Util.MakeRegex("width\\s*=\\s*(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                                middle = r2.Replace(middle, "");
                                Regex r3 = Util.MakeRegex("height\\s*=\\s*(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                                middle = r3.Replace(middle, "");
                            }

                            #if false
                            {
                                // remove tanorigwidth and tanorigheight
                                Regex r2 = Util.MakeRegex("tanorigwidth\\s*=\\s*(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*)", RegexOptions.IgnoreCase|RegexOptions.Compiled);
                                middle = r2.Replace(middle, "");
                                Regex r3 = Util.MakeRegex("tanorigheight\\s*=\\s*(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*)", RegexOptions.IgnoreCase|RegexOptions.Compiled);
                                middle = r3.Replace(middle, "");
                            }
                            #endif
                        }
                    }
                }

                return m.Groups[1].ToString() + middle + "/>";  //m.Groups[3].ToString();
            }

            private string AddTagQuotesStep2(Match m)
            {
                Log.Printn("--step2--");
                Log.Printn(m.Groups[1].ToString());
                return " " + m.Groups[1].ToString() + "=\"" + m.Groups[2].ToString() + "\"";
            }

            private string AddTagQuotesStep1(Match m)
            {
                string inside = m.Groups[2].ToString();

                Log.Printn("--inside--");
                Log.Printn(inside);

                Regex r = Util.MakeRegex("\\s+(?<1>\\w+)\\s*=\\s*(?!\")(?<2>.*?)(?=\\s|/|$)", RegexOptions.Compiled);

                inside = r.Replace(inside, new MatchEvaluator(this.AddTagQuotesStep2));

                return "<" + m.Groups[1].ToString() + inside + ">";
            }

            // for each postset
            public override bool operation(Object ob)
            {
                PostSet ps = (PostSet)ob;
                m_curPhotoService = ps.GetPhotoService();
                PostInfo postInfo = new PostInfo();

                // we have to filter the images BEFORE we convert them since there SRC tag will get modified
                foreach (string part in htmlParts)
                {
                    // get the global entry
                    string bod = m_entry.GetEntry(part);

                    // if there is a photo service let us filter the tags for that service in particular
                    if (m_curPhotoService != null)
                    {
                        bod = m_curPhotoService.FilterImageTags(m_entry, bod);
                    }

                    // save the entry for this service only
                    postInfo.PutEntry(part, bod);
                }

                if (m_curPhotoService != null)
                {
                    m_curPhotoService.Startup();

                    m_bgw.UpdateMessage( String.Format( "Posting to: {0}", ps.GetName() ));
                    m_bgw.UpdateProgress( m_itemNumber );
                    bs_refresh();

                    // -- upload each image for this service
                    {
                        uploadImageOp uiop = new uploadImageOp(m_entry, m_bgw, m_curPhotoService, m_itemNumber);
                        m_entry.OpOnHtmlImages(uiop);
                        m_itemNumber = uiop.m_itemNumber;
                    }

                    m_curPhotoService.Shutdown();

                    if (m_bgw.CancellationPending)
                    {
                        m_bgw.e.Cancel = true;
                        return false;
                    }
                }

                m_bgw.UpdateMessage( String.Format( "Posting to: {0}", ps.GetName() ));
                m_bgw.UpdateProgress( m_itemNumber );
                bs_refresh();

                foreach (string part in htmlParts)
                {
                    string bod = postInfo.GetEntry(part);

                    Log.Printn("----" + part + "----");
                    Log.Printn(bod);

                    // replace SRC, Width, Height tags
                    {
                        // find all <img src="">
                        Regex r = Util.MakeRegex("(?<1><img\\s+)(?<2>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[^>\"'/])*)(?<3>/*>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                        // Replace matched characters using the delegate method.
                        bod = r.Replace(bod, new MatchEvaluator(this.ReplaceSRCWidthHeight));
                    }

                    Log.Printn("----img-src----");
                    Log.Printn(bod);

                    // add quotes to html
                    {
                        // find all <> tags
                        Regex r = Util.MakeRegex("<(?<1>\\w+)(?<2>\\s(?:\"[^\"]*\"|'[^']*'|[^\"'>])*)>", RegexOptions.Compiled);

                        // Replace matched characters using the delegate method.
                        bod = r.Replace(bod, new MatchEvaluator(this.AddTagQuotesStep1));
                    }

                    Log.Printn("----after-(" + part + ")---");
                    Log.Printn(bod);
                    Log.Printn("------------");

                    postInfo.PutEntry(part, bod);
                }

                // and we have to encode each
                #if false
                if (ps.GetBlogService().CanEncode())
                {
                    foreach (string part in ps.GetEncodables())
                    {
                         // Create two different encodings.
                         int codepage = m_bps.GetIntEntry("encoding");
                         if (codepage == 0) { codepage = 65001; }
                         Encoding target  = Encoding();
                         Encoding unicode = Encoding.Unicode;

                         string orig = postInfo.GetEntry(part);
                         if (orig.Length == 0)
                         {
                             orig = m_entry.GetEntry(part);
                         }

                         // Convert the string into a byte[].
                         byte[] unicodeBytes = unicode.GetBytes(orig);

                         // Perform the conversion from one encoding to the other.
                         byte[] targetBytes = Encoding.Convert(unicode, target, unicodeBytes);

                         // Convert the new byte[] into a char[] and then into a string.
                         // This is a slightly different approach to converting to illustrate
                         // the use of GetCharCount/GetChars.
                         string targetString = target.GetString(targetBytes);
                    }
                }
                #endif

                try
                {
                    ps.GetBlogService().PostEntry(m_entry, postInfo);
                }
                catch (Exception ex)
                {
                    string err = String.Format(Localize.G.GetText("ErrorCantPostEntry"), ps.GetName());
                    throw new ApplicationException(err + "\n\n" + ex.Message);
                }

                if (m_bgw.CancellationPending)
                {
                    m_bgw.e.Cancel = true;
                    return false;
                }

                m_itemNumber++;
                return true;
            }
        }

        class countItems : ArrayListOperator
        {
            EntryInfo    m_entry;
            public int   m_numItems;
            int          m_publishCount;

            public countItems(EntryInfo entry)
            {
                m_entry = entry;
                m_numItems = 0;

                // get post count
                m_publishCount = entry.GetIntEntry("__publishCount") + 1;
                entry.PutIntEntry("__publishCount", m_publishCount);
            }

            public override bool operation(Object ob)
            {
                PostSet ps = (PostSet)ob;

                // check if we passed blog
                if (m_entry.GetIntEntry(ps.GetBlogService(), "__blogPublishCount") != m_publishCount)
                {
                    m_numItems++;
                    m_entry.PutIntEntry(ps.GetBlogService(), "__blogPublishCount", m_publishCount, true);
                }

                // this this service has a photo set and it hasn't already been
                // visited add the item
                if (ps.GetPhotoService() != null)
                {
                    if (m_entry.GetIntEntry(ps.GetPhotoService(), "__photoPublishCount") != m_publishCount)
                    {
                        // todo: I should walk the images and see if they need to be posted
                        m_numItems += m_entry.GetNumHtmlImages();
                        m_entry.PutIntEntry(ps.GetPhotoService(), "__photoPublishCount", m_publishCount, true);
                    }
                }

                return true;
            }
        }

        private void Publish(TanBackgroundWorker bgw)
        {
            int numItems;

            bs_refresh();
            // count the number of things we need to do
            {
                countItems ci = new countItems(m_entry);
                UserSettings.G.OpOnOnlyOnPostSets(ci);
                numItems = ci.m_numItems;
                if (numItems <= 0) numItems = 1;
            }
            bs_refresh();
            bgw.SetRange( numItems );
            bs_refresh();

            UserSettings.G.OpOnOnlyOnPostSets(new postToServices(m_entry, bgw));

            m_entry.PutEntry("__published__", "true");
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            // This method will run on a thread other than the UI thread.
            // Be sure not to manipulate any Windows Forms controls created
            // on the UI thread from this method.

            TanBackgroundWorker bgw = (TanBackgroundWorker)sender;
            bgw.e = e;
            bgw.e.Result = "";
            Publish(bgw);
        }

        private void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TanBackgroundWorker.ProgressInfo pi = (TanBackgroundWorker.ProgressInfo)e.UserState;

            switch (pi.m_cmd)
            {
            case TanBackgroundWorker.ProgressInfo.Cmd.SetRange:
                m_psf.SetRange(pi.m_value);
                break;
            case TanBackgroundWorker.ProgressInfo.Cmd.UpdateProgress:
                m_psf.SetProgress(pi.m_value);
                break;
            case TanBackgroundWorker.ProgressInfo.Cmd.UpdateMessage:
                m_psf.SetMessage(pi.m_msg);
                break;
            case TanBackgroundWorker.ProgressInfo.Cmd.Print:
                Log.RealPrintn(pi.m_msg);
                break;
            }

        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // this runs in the form's thread.  Close the dialog I guess
            m_psf.Close();
            m_psf.Dispose();
            Cursor = Cursors.Default;
            this.Enabled = true;

            if (e.Error != null)
            {
                MessageBox.Show(this, e.Error.Message);
            }

            Log.SetBackgroundWorker(null);
            syncPostButtons();
            this.wbBody.Focus();
        }

    }
}
