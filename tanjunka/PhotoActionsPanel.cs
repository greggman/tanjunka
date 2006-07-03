#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;

#endregion

// parts borrowed from
//      http://www.codeguru.com/Cpp/G-M/gdi/gdi/article.php/c3675/

namespace Tanjunka
{
    partial class BodyForm : Form
    {
        int _lastValue;
        ActionResults _drawResults;     // results for drawing
        ActionResults _inResults;       // results when we came into this tab
        PAOneValueColorAction _currentAction;
        string _picFileNameCommingIn;

        public void InitializeComponentPhotoEdit()
        {
            _drawResults = new ActionResults();
            _inResults = new ActionResults();

            this.trackBarBrightness.MouseDown += new MouseEventHandler(trackBarPhoto_MouseDown);
            this.trackBarBrightness.MouseMove += new MouseEventHandler(trackBarPhoto_MouseMove);
            this.trackBarBrightness.MouseUp += new MouseEventHandler(trackBarPhoto_MouseUp);
            this.trackBarBrightness.Tag = new ActionHandlerBrightness();

            this.trackBarContrast.MouseDown += new MouseEventHandler(trackBarPhoto_MouseDown);
            this.trackBarContrast.MouseMove += new MouseEventHandler(trackBarPhoto_MouseMove);
            this.trackBarContrast.MouseUp += new MouseEventHandler(trackBarPhoto_MouseUp);
            this.trackBarContrast.Tag = new ActionHandlerContrast();

            this.trackBarSat.MouseDown += new MouseEventHandler(trackBarPhoto_MouseDown);
            this.trackBarSat.MouseMove += new MouseEventHandler(trackBarPhoto_MouseMove);
            this.trackBarSat.MouseUp += new MouseEventHandler(trackBarPhoto_MouseUp);
            this.trackBarSat.Tag = new ActionHandlerSaturation();

            this.trackBarHue.MouseDown += new MouseEventHandler(trackBarPhoto_MouseDown);
            this.trackBarHue.MouseMove += new MouseEventHandler(trackBarPhoto_MouseMove);
            this.trackBarHue.MouseUp += new MouseEventHandler(trackBarPhoto_MouseUp);
            this.trackBarHue.Tag = new ActionHandlerHue();

            this.trackBarLight.MouseDown += new MouseEventHandler(trackBarPhoto_MouseDown);
            this.trackBarLight.MouseMove += new MouseEventHandler(trackBarPhoto_MouseMove);
            this.trackBarLight.MouseUp += new MouseEventHandler(trackBarPhoto_MouseUp);
            this.trackBarLight.Tag = new ActionHandlerBrightness();

            this.trackTintHue.MouseDown += new MouseEventHandler(trackBarPhoto_MouseDown);
            this.trackTintHue.MouseMove += new MouseEventHandler(trackBarPhoto_MouseMove);
            this.trackTintHue.MouseUp += new MouseEventHandler(trackBarPhoto_MouseUp);
            this.trackTintHue.Tag = new ActionHandlerTintHue();

            this.trackTintAmount.MouseDown += new MouseEventHandler(trackBarPhoto_MouseDown);
            this.trackTintAmount.MouseMove += new MouseEventHandler(trackBarPhoto_MouseMove);
            this.trackTintAmount.MouseUp += new MouseEventHandler(trackBarPhoto_MouseUp);
            this.trackTintAmount.Tag = new ActionHandlerTintAmount();

            this.trackBarGamma.MouseDown += new MouseEventHandler(trackBarPhoto_MouseDown);
            this.trackBarGamma.MouseMove += new MouseEventHandler(trackBarPhoto_MouseMove);
            this.trackBarGamma.MouseUp += new MouseEventHandler(trackBarPhoto_MouseUp);
            this.trackBarGamma.Tag = new ActionHandlerGamma();

            // quick hack :-(
            this.tabCrop.Tag   = 0;
            this.tabBright.Tag = 1;
            this.tabHSV.Tag    = 2;

            this.tabImage.KeyPress += new KeyPressEventHandler (this.photoKeyPress);
            this.photoPanel.CropDataChanged += new CropDataChangedEventHandler (this.photoCropChanged);
        }

        public void photoCropChanged (object sender, CropDataChangedEventArgs e)
        {
            UpdateCropResetUI();
        }

        public void trackBarPhoto_MouseDown(object sender, MouseEventArgs mea)
        {
            TrackBar tb = sender as TrackBar;
            ImageActionHandler ia = tb.Tag as ImageActionHandler;

            if (this.photoPanel.GetEditImage() != null)
            {
                _lastValue = tb.Value;
                tb.Capture = true;

                // create a new action saving the old value
                _currentAction = ia.CreateNewAction();
                _currentAction.Amount = _lastValue;
            }
        }

        public void trackBarPhoto_MouseMove(object sender, MouseEventArgs mea)
        {
            TrackBar tb = sender as TrackBar;

            if (tb.Capture)
            {
                if (tb.Value != _lastValue)
                {
                    _lastValue = tb.Value;
                    _currentAction.Amount = tb.Value;
                    this.photoPanel.GetEditImage().CopyResults(_drawResults);
                    _currentAction.EffectResults(_drawResults);
                    this.photoPanel.SetGamma(_drawResults.GetImageGamma());
                    this.photoPanel.SetColorMatrix(_drawResults.GetColorMatrix());
                }
            }
        }

        public void trackBarPhoto_MouseUp(object sender, MouseEventArgs mea)
        {
            TrackBar tb = sender as TrackBar;
            ImageActionHandler ia = tb.Tag as ImageActionHandler;

            tb.Capture = false; // this appears to be clear automatically

            if (this.photoPanel.GetEditImage() != null)
            {
                TJPagePicture tpic = this.photoPanel.GetEditImage();
                tpic.AddAction(_currentAction);
                if (ia.NeedToUpdateUI())
                {
                    tabUpdatePhotoUI();
                }
                UpdateUndoRedoUI();
                _currentAction = null;  // just to make sure
            }
        }

        private void tabBright_Resize(object sender, EventArgs e)
        {
            tabBright_realResize();
        }

        private void tabBright_realResize()
        {
            this.trackBarBrightness.Width = this.tabBright.Size.Width - 10;
            this.trackBarContrast.Width   = this.tabBright.Size.Width - 10;
            this.trackBarGamma.Width      = this.tabBright.Size.Width - 10;
        }

        private void tabHSV_Resize(object sender, EventArgs e)
        {
            tabHSV_realResize();
        }

        private void tabHSV_realResize()
        {
            this.trackBarHue.Width     = this.tabHSV.Size.Width - 10;
            this.trackBarSat.Width     = this.tabHSV.Size.Width - 10;
            this.trackBarLight.Width   = this.tabHSV.Size.Width - 10;
            this.trackTintHue.Width    = this.tabHSV.Size.Width - 10;
            this.trackTintAmount.Width = this.tabHSV.Size.Width - 10;
        }

        private void UpdateUndoRedoUI()
        {
            TJPagePicture tpic = this.photoPanel.GetEditImage();
            if (tpic != null)
            {
                this.paUndoButton.Enabled = tpic.HaveActions();
                this.paRedoButton.Enabled = tpic.HaveRedoActions();
            }
        }

        private void UpdateCropResetUI()
        {
            TJPagePicture tpic = this.photoPanel.GetEditImage();
            if (tpic != null)
            {
                this.cropButton.Enabled = !this.photoPanel.CropEmpty();
                this.resetCropButton.Enabled = !tpic.Uncropped();
            }
        }

        private void tabUpdatePhotoUI()
        {
            TJPagePicture tpic = this.photoPanel.GetEditImage();
            if (tpic != null)
            {
                tpic.CopyResults(_drawResults);

                this.trackBarContrast.Value   = _drawResults.Contrast;
                this.trackBarBrightness.Value = _drawResults.Brightness;
                this.trackBarSat.Value        = _drawResults.Saturation;
                this.trackBarHue.Value        = _drawResults.Hue;
                this.trackBarLight.Value      = _drawResults.Brightness;
                this.trackTintHue.Value       = _drawResults.TintHue;
                this.trackTintAmount.Value    = _drawResults.TintAmount;
                this.trackBarGamma.Value      = _drawResults.Gamma;

                this.photoPanel.SetGamma(_drawResults.GetImageGamma());
                this.photoPanel.SetColorMatrix(_drawResults.GetColorMatrix());

                UpdateUndoRedoUI();
                // this.paRevertButton
            }
        }

        private void updateCropUI()
        {
            TJPagePicture tpic = this.photoPanel.GetEditImage();
            if (tpic != null)
            {
                PhotoAction ac = tpic.TopRedoAction();
                if (ac != null)
                {
                    //if (ac.GetType() == PACrop)
                    //{
                    //    _drawResult.Reset();
                        //                  this.PhotoPanel.SetCropRect(_drawResult.GetIntRect(new Size(tpic.GetOrigWidth(), tpic.GetOrigHeight())));
                    //}
                }
            }
        }

        private void photoAction_revert(object sender, EventArgs e)
        {
            TJPagePicture tpic = this.photoPanel.GetEditImage();
            tpic.CopyResults(_drawResults);
            tpic.AddAction(new PARevert());
            if (!tpic.SameRect(_drawResults))
            {
                this.photoPanel.RegenerateImage();
            }
            this.photoPanel.Invalidate();
            tabUpdatePhotoUI();
            UpdateCropResetUI();
            updateCropUI();
        }

        private void photoAction_realUndo()
        {
            TJPagePicture tpic = this.photoPanel.GetEditImage();
            tpic.CopyResults(_drawResults);

            if (tpic.UndoAction())
            {
                if (!tpic.SameRect(_drawResults))
                {
                    this.photoPanel.RegenerateImage();
                }
                tabUpdatePhotoUI();
                UpdateCropResetUI();
                updateCropUI();
            }
        }

        private void photoAction_undo(object sender, EventArgs e)
        {
            photoAction_realUndo();
        }

        private void photoAction_realRedo()
        {
            TJPagePicture tpic = this.photoPanel.GetEditImage();
            tpic.CopyResults(_drawResults);
            if (tpic.RedoAction())
            {
                if (!tpic.SameRect(_drawResults))
                {
                    this.photoPanel.RegenerateImage();
                }
                tabUpdatePhotoUI();
                UpdateCropResetUI();
                updateCropUI();
            }
        }

        private void photoAction_redo(object sender, EventArgs e)
        {
            photoAction_realRedo();
        }

        private void photoAction_resetCrop(object sender, EventArgs e)
        {
            TJPagePicture tpic = this.photoPanel.GetEditImage();
            tpic.CopyResults(_drawResults);
            tpic.AddAction(new PAResetCrop());
            UpdateCropResetUI();
            UpdateUndoRedoUI();
            if (!tpic.SameRect(_drawResults))
            {
                this.photoPanel.RegenerateImage();
            }
            this.photoPanel.Invalidate();
        }

        private void photoAction_crop(object sender, EventArgs e)
        {
            Rectangle cropRect = this.photoPanel.GetCropRect();

            if (!cropRect.IsEmpty)
            {
                TJPagePicture tpic = this.photoPanel.GetEditImage();
                tpic.CopyResults(_drawResults);
                FloatRect curRect = _drawResults.CropRect;

                Size curCroppedSize = this.photoPanel.GetOrigSize();
                FloatRect fr = new FloatRect(curCroppedSize, cropRect);

                fr.Left   = curRect.Left + fr.Left * curRect.Width;
                fr.Top    = curRect.Top  + fr.Top  * curRect.Height;
                fr.Width  = curRect.Width  * fr.Width;
                fr.Height = curRect.Height * fr.Height;

                this.photoPanel.GetEditImage().AddAction(new PACrop(fr));
                UpdateUndoRedoUI();
                UpdateCropResetUI();
                this.photoPanel.RegenerateImage();
                this.photoPanel.Invalidate();
            }
        }

        private void photoAction_flipVert(object sender, EventArgs e)
        {
            this.photoPanel.GetEditImage().AddAction(new PAFlipVert());
            UpdateUndoRedoUI();
            UpdateCropResetUI();
            this.photoPanel.RegenerateImage();
            this.photoPanel.Invalidate();
        }

        private void photoAction_flipHoriz(object sender, EventArgs e)
        {
            this.photoPanel.GetEditImage().AddAction(new PAFlipHoriz());
            UpdateUndoRedoUI();
            UpdateCropResetUI();
            this.photoPanel.RegenerateImage();
            this.photoPanel.Invalidate();
        }

        private void photoAction_rotateCCW(object sender, EventArgs e)
        {
            this.photoPanel.GetEditImage().AddAction(new PARotateCCW());
            UpdateUndoRedoUI();
            this.photoPanel.RegenerateImage();
            this.photoPanel.Invalidate();
        }

        private void photoAction_rotateCW(object sender, EventArgs e)
        {
            this.photoPanel.GetEditImage().AddAction(new PARotateCW());
            UpdateUndoRedoUI();
            this.photoPanel.RegenerateImage();
            this.photoPanel.Invalidate();
        }

        private void photoActionsTabs_SetCropMode()
        {
            TabPage tp = this.photoActionsTabs.SelectedTab;
            int tabID = (int)tp.Tag;

            if (tabID == 0) // crop
            {
                this.photoPanel.SetCropMode();
            }
            else
            {
                this.photoPanel.SetDesignMode();
            }
        }

        private void UICommand_image(TanTabPage.UICommandID cmd)
        {
            if (this.photoPanel.GetEditImage() != null)
            {
                switch (cmd)
                {
                case TanTabPage.UICommandID.Undo:
                    photoAction_realUndo();
                    break;
                case TanTabPage.UICommandID.Redo:
                    photoAction_realRedo();
                    break;
                }
            }
        }

        private void photoKeyPress(object ob, KeyPressEventArgs e)
        {
            if (this.photoPanel.GetEditImage() != null)
            {
                switch (e.KeyChar)
                {
                case '\x1A':    // ctrl-z
                    photoAction_realUndo();
                    e.Handled = true;
                    break;
                case '\x19':    // ctrl-y
                    photoAction_realRedo();
                    e.Handled = true;
                    break;
                }
            }
        }

        private void photoActionsTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            photoActionsTabs_SetCropMode();
        }

        private void SetEditImage (TJPagePicture tpic)
        {
            this.photoPanel.SetEditImage(tpic);

            tabUpdatePhotoUI();
        }

        private void tabEnablePhotoUI()
        {
            bool bEnabled = (this.photoPanel.GetEditImage() != null);

            this.photoPanel.Enabled = bEnabled;
            this.photoActionsTabs.Enabled = bEnabled;
            this.splitContainer1.Enabled = bEnabled;
        }

        private void update_entryFromImage(EntryInfo entry)
        {
            TJPagePicture tpic = this.photoPanel.GetEditImage();
            if (tpic != null)
            {
                // only update if the results are different
                tpic.CopyResults(_drawResults);
                if (!_inResults.Same(_drawResults))
                {
                    tpic.BumpVersion(); // so it gets re-uploaded if it has already been uploaded
                    m_entry.resizeTempFileToFit(tpic, tpic.GetNewWidth(), tpic.GetNewHeight(), true);

                    // fix up html to match new image

                    foreach (string part in htmlParts)
                    {
                        string html = entry.GetEntry(part);

                        // find all <img src="">
                        Regex r = Util.MakeRegex("(?<1><img\\s+)(?<2>(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[^>\"'/])*)(?<3>/*>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                        // Replace matched characters using the delegate method.
                        html = r.Replace(html, new MatchEvaluator(this.ReplaceSRCWidthHeightForEditedImage));

                        entry.PutEntry(part, html);
                    }
                }
            }
        }

        private string ReplaceSRCWidthHeightForEditedImage(Match m)
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

                if (Path.GetFileName(src).CompareTo(_picFileNameCommingIn) == 0)
                {
                    TJPagePicture tjpic = this.photoPanel.GetEditImage();

                    string newSrc    = tjpic.GetTempPath();
                    string newWidth  = tjpic.GetNewWidth().ToString();
                    string newHeight = tjpic.GetNewHeight().ToString();

                    middle = r.Replace(middle, "src=\"" + newSrc + "\"");

                    if (newWidth.Length > 0 && newHeight.Length > 0)
                    {
                        Regex r2 = Util.MakeRegex("width\\s*=\\s*(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        middle = r2.Replace(middle, "width=\"" + newWidth + "\"");
                        Regex r3 = Util.MakeRegex("height\\s*=\\s*(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[\\S]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        middle = r3.Replace(middle, "height=\"" + newHeight + "\"");
                    }
                }
            }

            return m.Groups[1].ToString() + middle + "/>";  //m.Groups[3].ToString();
        }

        private void update_ImageFromEntry(EntryInfo entry)
        {
            TJPagePicture tpic = this.photoPanel.GetEditImage();
            if (tpic != null)
            {
                // save this off because it might change
                _picFileNameCommingIn = Path.GetFileName(tpic.GetTempPath());
                tpic.CopyResults(_inResults);
            }
            photoActionsTabs_SetCropMode();
            tabEnablePhotoUI();
        }
    }

    public abstract class ImageActionHandler
    {
        public abstract PAOneValueColorAction CreateNewAction();
        public virtual bool NeedToUpdateUI() { return false; }
    }

    public class ActionHandlerBrightness : ImageActionHandler
    {
        public override PAOneValueColorAction CreateNewAction()
        {
            return new PABrightness();
        }
        public override bool NeedToUpdateUI() { return true; }
    }

    public class ActionHandlerContrast : ImageActionHandler
    {
        public override PAOneValueColorAction CreateNewAction()
        {
            return new PAContrast();
        }
    }

    public class ActionHandlerSaturation : ImageActionHandler
    {
        public override PAOneValueColorAction CreateNewAction()
        {
            return new PASaturation();
        }
    }

    public class ActionHandlerHue : ImageActionHandler
    {
        public override PAOneValueColorAction CreateNewAction()
        {
            return new PAHue();
        }
    }

    public class ActionHandlerTintHue : ImageActionHandler
    {
        public override PAOneValueColorAction CreateNewAction()
        {
            return new PATintHue();
        }
    }

    public class ActionHandlerTintAmount : ImageActionHandler
    {
        public override PAOneValueColorAction CreateNewAction()
        {
            return new PATintAmount();
        }
    }

    public class ActionHandlerGamma : ImageActionHandler
    {
        public override PAOneValueColorAction CreateNewAction()
        {
            return new PAGamma();
        }
    }


}
