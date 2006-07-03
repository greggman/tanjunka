#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

#endregion

namespace Tanjunka
{
    public class PhotoPanel : Panel
    {
        private const int FrameSize = 2;
        private const int BorderSpace = 10;
        private readonly Color FrameColor = Color.FromArgb(240, 237, 219);
        private readonly Color CropDimColor = Color.FromArgb(100, 128, 128, 128);

        public const InterpolationMode WorkingInterpolationMode = InterpolationMode.Bilinear;
        public const InterpolationMode ViewingInterpolationMode = InterpolationMode.Bilinear;
        public const decimal WorkingScale = 0.65M;

        private Pen _penFrame;
        private Brush _brushDimCrop;

        private CropHelper _cropHelper;
        private Rectangle _photoBounds;

        private Size _orgPhotoSize;

        public event CropDataChangedEventHandler CropDataChanged;

        private ImageAttributes _imageAttr;

        enum Mode
        {
            Design,
            Crop,
        };

        private Mode mode;

        public PhotoPanel()
        {
            _penFrame = new Pen(FrameColor, FrameSize);
            _brushDimCrop = new SolidBrush(CropDimColor);
            _cropHelper = new CropHelper(this);
            _imageAttr = new ImageAttributes();

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint  |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint             |
                ControlStyles.ResizeRedraw          ,
                true);

            this.BackColor = Color.DarkGray;

            this.Name = "PhotoEdit";
            this.Text = "PhotoEdit";
            this.ResumeLayout(false);

            m_tpic = null;
            m_img  = null;

            ClearEditImage();
        }

        private TJPagePicture m_tpic;
        private Bitmap m_img;

        public void ClearEditImage()
        {
            if (m_img != null)
            {
                m_img.Dispose();
            }
            m_img  = null;
            m_tpic = null;

            #if false
            this.label1.Visible = true;
            this.rightRect.Visible = false;
            this.bottomRect.Visible = false;
            this.topRect.Visible = false;
            this.leftRect.Visible = false;
            this.pictureBox1.Visible = false;
            #endif
        }

        public void RegenerateImage()
        {
            if (m_tpic != null)
            {
                //m_img = new Bitmap(m_tpic.GetTempPath()); // this one holds a lock on the pic?
                m_img = m_tpic.GenerateEditImage();

                OnNewPhoto(m_img);
            }
        }

        public bool CropEmpty()
        {
            if (m_img == null) { return true; }
            return _cropHelper.Empty;
        }

        public void SetEditImage(TJPagePicture tpic)
        {
            if (m_img != null)
            {
                m_img.Dispose();
            }
            m_tpic = tpic;
            RegenerateImage();
        }

        public TJPagePicture GetEditImage()
        {
            return m_tpic;
        }

        public void SetCropMode()
        {
            mode = Mode.Crop;
            Invalidate();
//            OnCropDataChanged();
        }

        public void SetDesignMode()
        {
            mode = Mode.Design;
            Invalidate();
        }

        public void SetGamma(float gamma)
        {
            _imageAttr.SetGamma(gamma);
            this.Invalidate();
        }

        public void SetColorMatrix(ColorMatrix cm)
        {
            _imageAttr.SetColorMatrix(cm);
            this.Invalidate();
        }

        public void ClearColorMatrix(ColorMatrix cm)
        {
            _imageAttr.ClearGamma();
            _imageAttr.ClearColorMatrix();
            this.Invalidate();
        }

        public Size GetOrigSize() { return _orgPhotoSize; }
        public Rectangle GetCropRect() { return _cropHelper.OriginalPhotoSelectedArea; }

        public void ClearCrop()
        {
            // reset the photos bounds in the crop helper, this
            // marks the cropping selection area as empty, then
            // raise the event so the cropping coordinates in the
            // details pane can be updated
            _cropHelper.PhotoBounds = _photoBounds;
            OnCropDataChanged();

            // redraw without the cropping area
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (m_img == null)
            {
                pe.Graphics.Clear(BackColor);
                return;
            }

            DrawPhoto(pe.Graphics);

            DrawCrop(pe.Graphics);
        }

        // override to prevent windows from drawing the background
        protected override void OnPaintBackground(PaintEventArgs pe)
        {
        }

        private void OnNewPhoto(Bitmap srcImage)
        {
            // create the working image, this rescales the image
            // to a workable size if neccessary
            //CreateWorkingImage(srcImage)

            // save size of org image for cropping
            _cropHelper.PhotoBounds = _photoBounds;
            _orgPhotoSize = srcImage.Size;
            _cropHelper.OriginalPhotoSize = _orgPhotoSize;

            OnCropDataChanged();
        }

        // draw the current photo, stretching it to fit in the available area
        private void DrawPhoto(Graphics g)
        {
            g.Clear(BackColor);

            if (m_img != null)
            {
                Rectangle bounds = DisplayRectangle;
                bounds.Inflate(-BorderSpace, -BorderSpace);

                int boundsWidth = Math.Max(bounds.Width, 0);
                int boundsHeight = Math.Max(bounds.Height, 0);
                decimal ratio = Math.Max(((decimal)m_img.Width / (decimal)boundsWidth), ((decimal)m_img.Height / (decimal)boundsHeight));

                int width = (int)(((decimal)m_img.Width) / ratio);
                int height = (int)(((decimal)m_img.Height) / ratio);

                int x = (this.Width - width) / 2;
                int y = (this.Height - height) / 2;

                g.InterpolationMode = ViewingInterpolationMode;
                Rectangle rcDest = new Rectangle(x, y, width, height);

                g.DrawImage(m_img, rcDest, 0, 0, m_img.Width, m_img.Height, GraphicsUnit.Pixel, _imageAttr);
                //  g.DrawImage(m_img, rcDest, 0, 0, m_img.Width, m_img.Height, GraphicsUnit.Pixel);

                g.DrawRectangle(_penFrame, rcDest);

                // see if the photo drawing area changed, raise an
                // event so the main frame can update other objects
                if (_photoBounds.Equals(rcDest) == false)
                {
                    // store the new drawing area
                    _photoBounds = rcDest;

                    // notify the crop helper that the bounds has changed, also
                    // raise event so other areas of the app can be updated
                    _cropHelper.PhotoBounds = rcDest;
//                    OnCropDataChanged();
                }
            }
        }

        // ---------- croping ---------------

        protected override void OnMouseDown (MouseEventArgs e)
        {
            if (m_img == null || mode != Mode.Crop)
            {
                this.Focus();
                base.OnMouseDown(e);
                return;
            }

            base.OnMouseDown(e);

            if (Control.MouseButtons != MouseButtons.Left) { return; }

            this.Capture = true;

            Invalidate();
            Update();

            _cropHelper.MouseDown(e.X, e.Y);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (m_img == null || mode != Mode.Crop)
            {
                return;
            }

            if (this.Capture)
            {
                _cropHelper.MouseMove(e.X, e.Y);
//                OnCropDataChanged();
            }
            else
            {
                _cropHelper.SetCursor(e.X, e.Y);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (m_img == null)
            {
                return;
            }

            this.Capture = false;
            _cropHelper.MouseUp(e.X, e.Y);
            OnCropDataChanged();

            Invalidate();
        }

        // fires the crop data changed event so other parts
        // of the app can be notified (update the coordinates
        // in the details pane)
        private void OnCropDataChanged()
        {
            Rectangle cropBounds = _cropHelper.OriginalPhotoSelectedArea;

            if (CropDataChanged != null)
            {
                CropDataChanged(this, new CropDataChangedEventArgs(_cropHelper.OriginalPhotoSize, cropBounds.Size, cropBounds));
            }
        }

        // draw the cropping area by drawing a dimmed area on the entire
        // image except the cropping area
        private void DrawCrop(Graphics g)
        {
            if (this.Capture || m_img == null)
            {
                return;
            }

            if (_cropHelper.Empty || mode != Mode.Crop)
            {
                return;
            }

            Rectangle area = _cropHelper.SelectedArea;

            System.Drawing.Region region = new System.Drawing.Region(_photoBounds);
            region.Exclude(area);
            g.FillRegion(_brushDimCrop, region);
            region.Dispose();

            ControlPaint.DrawFocusRectangle(g, _cropHelper.SelectedArea, Color.White, Color.Black);
        }
    }

    public delegate void CropDataChangedEventHandler(object sender, CropDataChangedEventArgs e);

    public class CropHelper
    {
        private enum DragMode
        {
            None,
            Move,
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left,
        }

        private static readonly Cursor[] CursorLookup = {
            Cursors.Cross,
            Cursors.SizeAll,
            Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW, Cursors.SizeWE,
            Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW, Cursors.SizeWE,
            };

        // everything is stored in client coordinates
        private bool _mouseCaptured;
        private Point _originalPoint = new Point();
        private Point _lastPoint = new Point();
        private Rectangle _selArea = new Rectangle();
        private Rectangle _photoBounds = new Rectangle();
        private bool _empty = true;
        private Control _parent;
        private DragMode _dragMode = DragMode.None;
        private bool _drewFrame = false;
        private Size _originalPhotoSize = new Size();

        public bool Empty { get { return _empty; } set { _empty = value; } }
        public Size OriginalPhotoSize { get { return _originalPhotoSize; } set { _originalPhotoSize = value; } }
        public Rectangle PhotoBounds
        {
            get
            {
                return _photoBounds;
            }
            set
            {
                    _photoBounds = value;

                    // reset crop data
                    _selArea.X = _photoBounds.Left;
                    _selArea.Y = _photoBounds.Top;
                    _selArea.Width  = 0;
                    _selArea.Height = 0;
                    Empty = true;
            }
        }
        public Rectangle SelectedArea { get { return _selArea; } }
        public Rectangle OriginalPhotoSelectedArea
        {
            get
            {
                if (_photoBounds.Width == 0 || _photoBounds.Height == 0)
                {
                    return new Rectangle(0,0,0,0);
                }

                Rectangle area = NormalizeArea(_selArea);
                decimal x = (decimal)(_originalPhotoSize.Width) / (decimal)(_photoBounds.Width);
                decimal y = (decimal)(_originalPhotoSize.Height) / (decimal)(_photoBounds.Height);
                return new Rectangle(
                    (int)((decimal)(area.Left - _photoBounds.Left) * x),
                    (int)((decimal)(area.Top  - _photoBounds.Top ) * y),
                    (int)((decimal)(area.Width) * x),
                    (int)((decimal)(area.Height) * y));
            }
        }
        public Rectangle SelectedAreaScreen
        {
            get
            {
                return _parent.RectangleToScreen(_selArea);
            }
        }

        public CropHelper(Control parent)
        {
            _parent = parent;
        }

        public void MouseDown (int x, int y)
        {
            if (!_photoBounds.Contains(x, y))
            {
                return;
            }

            _mouseCaptured = true;

            Cursor.Current = CursorLookup[(int)_dragMode];

            if (_dragMode == DragMode.None)
            {
                // store the starting point for selection rectangle
                _originalPoint.X = x;
                _originalPoint.Y = y;
                _drewFrame = false;
            }
            else
            {
                _lastPoint.X = x;
                _lastPoint.Y = y;

                this.DrawReversibleRectangle();
                _drewFrame = true;
            }
        }

        public void MouseUp(int x, int y)
        {
            if (!_mouseCaptured) { return; }

            // done caputing the mouse movements
            _mouseCaptured = false;

            if (_dragMode == DragMode.None)
            {
                // erase the frame
                if (_drewFrame)
                {
                    DrawReversibleRectangle(_originalPoint, _lastPoint);
                }
            }
            else
            {
                if (_drewFrame)
                {
                    DrawReversibleRectangle();
                }
            }

            // always store normalized area
            _selArea = this.NormalizeArea(_selArea);

            // see if an area has been marked
            this.Empty = !(_selArea.Width > 0 && _selArea.Height > 0);
        }

        public void MouseMove(int x, int y)
        {
            if (!_mouseCaptured) { return; }

            // set cursor for current drag mode
            Cursor.Current = CropHelper.CursorLookup[(int)_dragMode];

            // draw the frame
            if (_dragMode == DragMode.None)
            {
                if (_drewFrame)
                {
                    DrawReversibleRectangle(_originalPoint, _lastPoint);
                }

                if (x >= _photoBounds.Left && x <= _photoBounds.Right)
                {
                    _lastPoint.X = x;
                }

                if (y >= _photoBounds.Top && y <= _photoBounds.Bottom)
                {
                    _lastPoint.Y = y;
                }

                DrawReversibleRectangle(_originalPoint, _lastPoint);
                _drewFrame = true;

                return;
            }

            // erase the current frame
            DrawReversibleRectangle();

            // calculate amount moved since last time
            int dx = x - _lastPoint.X;
            int dy = y - _lastPoint.Y;

            Rectangle newArea = _selArea;

            // update the area
            switch (_dragMode)
            {
            case DragMode.Move:
                newArea.Offset(dx, dy);
                break;
            case DragMode.Top:
                newArea.Y += dy;
                newArea.Height -= dy;
                break;
            case DragMode.Left:
                newArea.X += dx;
                newArea.Width -= dx;
                break;
            case DragMode.BottomRight:
                newArea.Width += dx;
                newArea.Height += dy;
                break;
            case DragMode.BottomLeft:
                newArea.X += dx;
                newArea.Width -= dx;
                newArea.Height += dy;
                break;
            case DragMode.TopLeft:
                newArea.Y += dy;
                newArea.Height -= dy;
                newArea.X += dx;
                newArea.Width -= dx;
                break;
            case DragMode.TopRight:
                newArea.Y += dy;
                newArea.Height -= dy;
                newArea.Width += dx;
                break;
            case DragMode.Right:
                newArea.Width += dx;
                break;
            case DragMode.Bottom:
                newArea.Height += dy;
                break;
            }

            // need to normalize the rect before calling .Contains
            Rectangle area = NormalizeArea(newArea);
            if (area.Left >= _photoBounds.Left && area.Right <= _photoBounds.Right)
            {
                _selArea.X = newArea.X;
                _selArea.Width = newArea.Width;
            }

            if (area.Top >= _photoBounds.Top && area.Bottom <= _photoBounds.Bottom)
            {
                _selArea.Y = newArea.Y;
                _selArea.Height = newArea.Height;
            }

            if (x >= _photoBounds.Left && x <= _photoBounds.Right)
            {
                _lastPoint.X = x;
            }

            if (y >= _photoBounds.Top && y <= _photoBounds.Bottom)
            {
                _lastPoint.Y = y;
            }

            // draw the new frame
            DrawReversibleRectangle();
        }

        // determine what sizing cursor to display
        public void SetCursor(int x, int y)
        {
            _dragMode = DragModeHitTest(this.SelectedArea, x, y);
            Cursor.Current = CropHelper.CursorLookup[(int)_dragMode];
        }

        #if false
        // the photo was rotated, so the original photo size
        // width and height need to be swapped
        public void OriginalPhotoRotated()
        {
            int width = _originalPhotoSize.Width;
            _originalPhotoSize.Width = _originalPhotoSize.Height;
            _originalPhotoSize.Height = width;
        }
        #endif

        // --- internal methods ---

        // do hit testing to determine if over the sizing frame
        private DragMode DragModeHitTest(Rectangle area, int x, int y)
        {
            const int space = 10;

            // not in area
            if (!area.Contains(x, y))
            {
                return DragMode.None;
            }

            // top left
            if (Math.Abs((x - area.Left)) + Math.Abs((y - area.Top)) < space )
            {
                return DragMode.TopLeft;
            }

            // top right
            if (Math.Abs((x - area.Right)) + Math.Abs((y - area.Top)) < space )
            {
                return DragMode.TopRight;
            }

            // bottom left
            if (Math.Abs((x - area.Left)) + Math.Abs((y - area.Bottom)) < space )
            {
                return DragMode.BottomLeft;
            }

            // bottom right
            if (Math.Abs((x - area.Right)) + Math.Abs((y - area.Bottom)) < space )
            {
                return DragMode.BottomRight;
            }

            // top line
            if (Math.Abs((y - area.Top)) < space )
            {
                return DragMode.Top;
            }

            // bottom line
            if (Math.Abs((y - area.Bottom)) < space )
            {
                return DragMode.Bottom;
            }

            // left
            if (Math.Abs((x - area.Left)) < space )
            {
                return DragMode.Left;
            }

            // right
            if (Math.Abs((x - area.Right)) < space )
            {
                return DragMode.Right;
            }

            // in area but not on the frame
            return DragMode.Move;
        }

        // convert and normalize the points and draw the reversible frame
        protected virtual void DrawReversibleRectangle(Point p1, Point p2)
        {
            _selArea.X = p1.X;
            _selArea.Y = p1.Y;
            _selArea.Width = p2.X - p1.X;
            _selArea.Height = p2.Y - p1.Y;

            DrawReversibleRectangle();
        }

        // draw reversible frame
        protected virtual void DrawReversibleRectangle()
        {
            ControlPaint.DrawReversibleFrame(this.SelectedAreaScreen, Color.Gray, FrameStyle.Dashed);
        }

        // work with normalized rects
        private Rectangle NormalizeArea(Rectangle area)
        {
            Rectangle newArea = area;

            if (area.Width < 0)
            {
                newArea.X = area.Right;
                newArea.Width = -area.Width;
            }

            if (area.Height < 0)
            {
                newArea.Y = area.Bottom;
                newArea.Height = -area.Height;
            }

            return newArea;
        }
    }

    public class CropDataChangedEventArgs : EventArgs
    {
        private Size _orgSize;
        private Size _newSize;
        private Rectangle _cropBounds;

        public Size OrgSize { get { return _orgSize; } }
        public Size NewSize { get { return _newSize; } }
        public Rectangle CropBounds { get { return _cropBounds; } }

        public CropDataChangedEventArgs (Size orgSize, Size newSize, Rectangle cropBounds)
        {
            _orgSize = orgSize;
            _newSize = newSize;
            _cropBounds = cropBounds;
        }
    }

}