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
using System.Runtime.Serialization;

#endregion

namespace Tanjunka
{
    public class ActionResults
    {
        private int    _contrast;   // (-100 to 100)
        private int    _brightness; // (-100 to 100)
        private int    _saturation; // (-100 to 100)
        private int    _hue;        // (-180 to 180)
        private int    _tintHue;    // (-180 to 180)
        private int    _tintAmount; // (   0 to 100)
        private int    _gamma;      // (  20 to 400)
        private bool   _flipHorizontal;
        private bool   _flipVertical;
        private int    _rotate;     // 0-3
        private FloatRect _cropRect;   // 0.0 to 1.0

        private static readonly RotateFlipType[] _rotateFlipTable =
            {
                RotateFlipType.RotateNoneFlipNone,
                RotateFlipType.Rotate90FlipNone,
                RotateFlipType.Rotate180FlipNone,
                RotateFlipType.Rotate270FlipNone,
                RotateFlipType.RotateNoneFlipX,
                RotateFlipType.Rotate90FlipX,
                RotateFlipType.Rotate180FlipX,
                RotateFlipType.Rotate270FlipX,
                RotateFlipType.RotateNoneFlipY,
                RotateFlipType.Rotate90FlipY,
                RotateFlipType.Rotate180FlipY,
                RotateFlipType.Rotate270FlipY,
                RotateFlipType.RotateNoneFlipXY,
                RotateFlipType.Rotate90FlipXY,
                RotateFlipType.Rotate180FlipXY,
                RotateFlipType.Rotate270FlipXY,
            };

        public ActionResults()
        {
            Reset();
        }

        public void CopyTo (ActionResults dst)
        {
            dst.Contrast = Contrast;
            dst.Brightness = Brightness;
            dst.Saturation = Saturation;
            dst.Hue = Hue;
            dst.TintHue = TintHue;
            dst.TintAmount = TintAmount;
            dst.Gamma = Gamma;
            dst.FlipHorizontal = FlipHorizontal;
            dst.FlipVertical = FlipVertical;
            dst.CropRect.Copy(CropRect);
            dst._rotate = _rotate;
        }

        public void Reset()
        {
            _contrast   = 0;
            _brightness = 0;
            _saturation = 0;
            _hue = 0;
            _tintHue = 0;
            _tintAmount = 0;
            _gamma = 100;
            _rotate = 0;
            _flipHorizontal = false;
            _flipVertical = false;
            _cropRect = new FloatRect();
        }

        public int  Contrast { get { return _contrast; } set { _contrast = value; } }
        public int  Brightness { get { return _brightness; } set { _brightness = value; } }
        public int  Saturation { get { return _saturation; } set { _saturation = value; } }
        public int  Hue { get { return _hue; } set { _hue = value; } }
        public int  TintHue { get { return _tintHue; } set { _tintHue = value; } }
        public int  TintAmount { get { return _tintAmount; } set { _tintAmount = value; } }
        public int  Gamma { get { return _gamma; } set { _gamma = value; } }
        public bool FlipHorizontal { get { return _flipHorizontal; } set { _flipHorizontal = value; } }
        public bool FlipVertical { get { return _flipVertical; } set { _flipVertical = value; } }
        public FloatRect CropRect { get { return _cropRect; } set { _cropRect = value; } }

        public RotateFlipType RotateFlip
        {
            get
            {
                return _rotateFlipTable[_rotate + (_flipHorizontal ? 4 : 0) + (_flipVertical ? 8 : 0)];
            }
        }

        public ColorMatrix GetColorMatrix()
        {
            // * contrast
            // * brightness
            // * saturation
            // * hue
            // * tint

            ColorMatrix m = ImageUtil.GetContrastMatrix(Contrast);
            m = ImageUtil.CombineMatrix(m, ImageUtil.GetBrightnessMatrix(Brightness)   );
            m = ImageUtil.CombineMatrix(m, ImageUtil.GetSaturationMatrix(Saturation));
            m = ImageUtil.CombineMatrix(m, ImageUtil.GetRotateHueMatrix(Hue));
            m = ImageUtil.CombineMatrix(m, ImageUtil.GetTintMatrix(TintHue, TintAmount));

            return m;
        }

        public bool Uncropped()
        {
            return (_rotate == 0 &&
                    FlipHorizontal == false &&
                    FlipVertical == false &&
                    CropRect.Left == 0.0 &&
                    CropRect.Top == 0.0 &&
                    CropRect.Width == 1.0 &&
                    CropRect.Height == 1.0);
        }

        public bool Same(ActionResults other)
        {
            return CropRect.Same(other.CropRect) &&
                other.Contrast == Contrast &&
                other.Brightness == Brightness &&
                other.Saturation == Saturation &&
                other.Hue == Hue &&
                other.TintHue == TintHue &&
                other.TintAmount == TintAmount &&
                other.Gamma == Gamma &&
                other.FlipHorizontal == FlipHorizontal &&
                other.FlipVertical == FlipVertical &&
                other._rotate == _rotate;
        }

        public bool SameRect(ActionResults other)
        {
            return CropRect.Same(other.CropRect) && (other._rotate == _rotate);
        }

        public void RotateCW()
        {
            if (FlipHorizontal ^ FlipVertical)
            {
                _cropRect.RotateCCW();
            }
            else
            {
                _cropRect.RotateCW();
            }
            _rotate += 1;
            _rotate &= 3;
        }

        public void RotateCCW()
        {
            if (FlipHorizontal ^ FlipVertical)
            {
                _cropRect.RotateCW();
            }
            else
            {
                _cropRect.RotateCCW();
            }
            _rotate += 3;
            _rotate &= 3;
        }

        public void ClearRotate()
        {
            _rotate = 0;
        }

        public Rectangle GetRotatedIntRect(Size origSize)
        {
            Size rotatedSize;

            if ((_rotate & 0x1) == 0x1) // rotate
            {
                rotatedSize = new Size(origSize.Height, origSize.Width);
            }
            else
            {
                rotatedSize = new Size(origSize.Width, origSize.Height);
            }

            return _cropRect.GetIntRect(rotatedSize);
        }

        public float GetImageGamma()
        {
            float g = _gamma / 100.0f;
            return (g < 0.2f) ? 0.2f : g;
        }
    }


    [Serializable]
    public abstract class PhotoAction : ISerializable
    {
        public PhotoAction ()
        {
        }

        public PhotoAction(SerializationInfo info, StreamingContext context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }

        public void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }

        public abstract void EffectResults(ActionResults ar);
    }

    [Serializable]
    public abstract class PAOneValueColorAction : PhotoAction, ISerializable
    {
        private int _amount;

        public PAOneValueColorAction()
        {
        }

        public PAOneValueColorAction(SerializationInfo info, StreamingContext context)
         : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            _amount = genericInfo.GetValue<int>("_amount");
        }

        public new void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);

            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("_amount", _amount);
        }

        public int Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                _amount = value;
            }
        }

    }

    [Serializable]
    public class PABrightness : PAOneValueColorAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.Brightness = Amount;
        }
        public PABrightness() { }
        public PABrightness(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }

    [Serializable]
    public class PAContrast : PAOneValueColorAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.Contrast = Amount;
        }
        public PAContrast() {   }
        public PAContrast(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }

    [Serializable]
    public class PASaturation : PAOneValueColorAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.Saturation = Amount;
        }
        public PASaturation() { }
        public PASaturation(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }

    [Serializable]
    public class PAHue : PAOneValueColorAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.Hue = Amount;
        }
        public PAHue() {    }
        public PAHue(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }


    [Serializable]
    public class PATintHue : PAOneValueColorAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.TintHue = Amount;
        }
        public PATintHue() {    }
        public PATintHue(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }

    [Serializable]
    public class PATintAmount : PAOneValueColorAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.TintAmount = Amount;
        }
        public PATintAmount() { }
        public PATintAmount(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }

    [Serializable]
    public class PAGamma : PAOneValueColorAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.Gamma = Amount;
        }
        public PAGamma() {  }
        public PAGamma(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }

    [Serializable]
    public class PACrop : PhotoAction, ISerializable
    {
        FloatRect _cropRect;

        public PACrop(FloatRect rc)
        {
            _cropRect = new FloatRect(rc);
        }

        public override void EffectResults(ActionResults ar)
        {
            ar.CropRect.Copy(_cropRect);
        }

        public PACrop(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            _cropRect = genericInfo.GetValue<FloatRect>("_cropRect");
        }

        public new void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);

            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("_cropRect", _cropRect);
        }
    }

    [Serializable]
    public class PAFlipHoriz : PhotoAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.FlipHorizontal = !ar.FlipHorizontal;
            ar.CropRect.FlipHoriz();
        }
        public PAFlipHoriz() {  }
        public PAFlipHoriz(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }

    [Serializable]
    public class PAFlipVert : PhotoAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.FlipVertical = !ar.FlipVertical;
            ar.CropRect.FlipVert();
        }
        public PAFlipVert() {   }
        public PAFlipVert(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }

    [Serializable]
    public class PAResetCrop : PhotoAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.CropRect.Copy(new FloatRect());
            ar.FlipVertical = false;
            ar.FlipHorizontal = false;
            ar.ClearRotate();
        }
        public PAResetCrop() {  }
        public PAResetCrop(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }

    [Serializable]
    public class PARevert : PhotoAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.Reset();
        }
        public PARevert() {  }
        public PARevert(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }

    [Serializable]
    public class PARotateCCW : PhotoAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.RotateCCW();
        }
        public PARotateCCW() {  }
        public PARotateCCW(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }

    [Serializable]
    public class PARotateCW : PhotoAction, ISerializable
    {
        public override void EffectResults(ActionResults ar)
        {
            ar.RotateCW();
        }
        public PARotateCW() {   }
        public PARotateCW(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);
        }
    }

    public class ImageUtil
    {
        public const float GrayRed   = 0.3086f;
        public const float GrayGreen = 0.6094f;
        public const float GrayBlue  = 0.082f;

        private static bool initialized = false;
        private static ColorMatrix preHue;
        private static ColorMatrix postHue;

        public static ColorMatrix GetBrightnessMatrix(int percent)
        {
            ColorMatrix m = new ColorMatrix();

            float v = (float)percent / 100.0f;
            m[4, 0] = v;
            m[4, 1] = v;
            m[4, 2] = v;

            return m;
        }

        public static ColorMatrix GetContrastMatrix(int percent)
        {
            ColorMatrix m = new ColorMatrix();

            float s;
            if (percent < 0)
            {
                s = (percent + 100) / 100.0f;
            }
            else
            {
                s = percent * 4.0f / 100.0f + 1.0f;
            }

            m[0, 0] = s;
            m[1, 1] = s;
            m[2, 2] = s;

            return m;
        }

        public static ColorMatrix GetSaturationMatrix(int percent)
        {
            float saturation;
            if (percent < 0)
            {
                saturation = (percent + 100) / 100.0f;
            }
            else
            {
                saturation = percent * 4.0f / 100.0f + 1.0f;
            }

            float satCompl = 1.0f - saturation;
            float satComplR = GrayRed   * satCompl;
            float satComplG = GrayGreen * satCompl;
            float satComplB = GrayBlue  * satCompl;

            float[][] matrix = new float[][]{
                new float[]{ satComplR + saturation, satComplR, satComplR, 0, 0, },
                new float[]{ satComplG, satComplG + saturation, satComplG, 0, 0, },
                new float[]{ satComplB, satComplB, satComplB + saturation, 0, 0, },
                new float[]{ 0, 0, 0, 1, 0, },
                new float[]{ 0, 0, 0, 0, 1, },
            };

            return new ColorMatrix(matrix);
        }

        public static ColorMatrix GetRotateHueMatrix(int degrees)
        {
            InitRotateHueMatrix();

            ColorMatrix m = new ColorMatrix();

            m = CombineMatrix(m, preHue);
            m = CombineMatrix(m, GetRotateBlueMatrix(degrees));
            return CombineMatrix(m, postHue);
        }

        public static ColorMatrix GetTintMatrix(int angle, int amount)
        {
            InitRotateHueMatrix();

            ColorMatrix m = new ColorMatrix();
            // Rotate hue.
            m = GetRotateHueMatrix(angle);

            // Scale blue
            m = CombineMatrix(m, GetScaleMatrix(1.0f, 1.0f, 1.0f + (float)amount / 100.0f, 1.0f));

            // Rotate hue back.
            return CombineMatrix(m, GetRotateHueMatrix(- angle));
        }

        private static void InitRotateHueMatrix()
        {
            const float greenRotation = 35.0f;
            //  const float greenRotation = 39.182655f;

            // NOTE: theoretically, greenRotation should have the value of 39.182655 degrees,
            // being the angle for which the sine is 1/(sqrt(3)), and the cosine is sqrt(2/3).
            // However, I found that using a slightly smaller angle works better.
            // In particular, the greys in the image are not visibly affected with the smaller
            // angle, while they deviate a little bit with the theoretical value.
            // An explanation escapes me for now.
            // If you rather stick with the theory, change the comments in the previous lines.

            if (! initialized)
            {
                initialized = true;
                // Rotating the hue of an image is a rather convoluted task, involving several matrix
                // multiplications. For efficiency, we prepare two static matrices.
                // This is by far the most complicated part of this class. For the background
                // theory, refer to the sgi-sites mentioned at the top of this file.

                // Prepare the preHue matrix.
                // Rotate the grey vector in the green plane.
                preHue = GetRotateRedMatrix(45.0f);

                // Next, rotate it again in the green plane, so it coincides with the blue axis.
                preHue = CombineMatrix(preHue, GetRotateGreenMatrix(-greenRotation));

                // Hue rotations keep the color luminations constant, so that only the hues change
                // visible. To accomplish that, we shear the blue plane.
                float[] lum = { GrayRed, GrayGreen, GrayBlue, 1.0f };

                // Transform the luminance vector.
                TransformVector(ref preHue, ref lum);

                // Calculate the shear factors for red and green.
                float red   = lum[0] / lum[2];
                float green = lum[1] / lum[2];

                // Shear the blue plane.
                preHue = CombineMatrix(preHue, GetShearBlueMatrix(red, green));

                // Prepare the postHue matrix. This holds the opposite transformations of the
                // preHue matrix. In fact, postHue is the inversion of preHue.
                postHue = GetShearBlueMatrix(- red, - green);
                postHue = CombineMatrix(postHue, GetRotateGreenMatrix(greenRotation));
                postHue = CombineMatrix(postHue, GetRotateRedMatrix(- 45.0f));
            }
        }

        private static void TransformVector(ref ColorMatrix m, ref float[] v)
        {
            float[] temp = new float[] { 0, 0, 0, 0 };

            for (int x = 0; x < 4; x++)
            {
                temp[x] = m[4,x];
                for (int y = 0; y < 4; y++)
                {
                    temp[x] += v[y] * m[y, x];
                }
            }
            for (int x = 0; x < 4; x++)
            {
                v[x] = temp[x];
            }
        }

        private static ColorMatrix GetScaleMatrix(
            float scaleRed,
            float scaleGreen,
            float scaleBlue,
            float scaleOpacity)
        {
            ColorMatrix m = new ColorMatrix();;

            m[0,0] = scaleRed;
            m[1,1] = scaleGreen;
            m[2,2] = scaleBlue;
            m[3,3] = scaleOpacity;

            return m;
        }

        // Rotate the matrix around one of the color axes. The color of the rotation
        // axis is unchanged, the other two colors are rotated in color space.
        // The angle phi is in degrees (-180.0f... 180.0f).
        private static ColorMatrix GetRotateRedMatrix(float angle)
        {
            return GetRotateColorMatrix(angle, 2, 1);
        }

        private static ColorMatrix GetRotateGreenMatrix(float angle)
        {
            return GetRotateColorMatrix(angle, 0, 2);
        }

        private static ColorMatrix GetRotateBlueMatrix(float angle)
        {
            return GetRotateColorMatrix(angle, 1, 0);
        }

        // Shear the matrix in one of the color planes. The color of the color plane
        // is influenced by the two other colors.
        private static ColorMatrix GetShearRedMatrix(float green, float blue)
        {
            return GetShearColorMatrix(0, 1, green, 2, blue);
        }

        private static ColorMatrix GetShearGreenMatrix(float red, float blue)
        {
            return GetShearColorMatrix(1, 0, red, 2, blue);
        }

        private static ColorMatrix GetShearBlueMatrix(float red, float green)
        {
            return GetShearColorMatrix(2, 0, red, 1, green);
        }

        private static ColorMatrix GetShearColorMatrix(int x, int y1, float d1, int y2, float d2)
        {
            ColorMatrix m = new ColorMatrix();
            m[y1,x] = d1;
            m[y2,x] = d2;

            return m;
        }

        // phi is in degrees
        // x and y are the indices of the value to receive the sin(phi) value
        private static ColorMatrix GetRotateColorMatrix(float angle, int x, int y)
        {
            float rad = angle * (float)Math.PI / 180.0f;

            ColorMatrix m = new ColorMatrix();

            m[x,x] = m[y,y] = (float)Math.Cos(rad);

            float s = (float)Math.Sin(rad);
            m[y,x] = s;
            m[x,y] = - s;

            return m;
        }

        // return a matrix that is the combination of the two specified matrixes,
        // maintains the order information of the matrix
        public static ColorMatrix CombineMatrix(ColorMatrix m1, ColorMatrix m2)
        {
            ColorMatrix matrix = new ColorMatrix();

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    matrix[y,x] =
                     m1[y,0] * m2[0,x] +
                     m1[y,1] * m2[1,x] +
                     m1[y,2] * m2[2,x] +
                     m1[y,3] * m2[3,x] +
                     m1[y,4] * m2[4,x] ;
                }
            }

            return matrix;
        }
    }

    [Serializable]
    public class FloatRect
    {
        public double Left;
        public double Top;
        public double Width;
        public double Height;

        public FloatRect()
        {
            Left   = 0.0;
            Top    = 0.0;
            Width  = 1.0;
            Height = 1.0;
        }

        public FloatRect(FloatRect fr)
        {
            Copy(fr);
        }

        public FloatRect(Size origSize, Rectangle intRect)
        {
             Left   = (double)intRect.Left   / (double)origSize.Width;
             Top    = (double)intRect.Top    / (double)origSize.Height;
             Width  = (double)intRect.Width  / (double)origSize.Width;
             Height = (double)intRect.Height / (double)origSize.Height;
        }

        public void Copy(FloatRect other)
        {
            Left   = other.Left;
            Top    = other.Top;
            Width  = other.Width;
            Height = other.Height;
        }

        public Rectangle GetIntRect(Size origSize)
        {
            return new Rectangle(
                (int)(Left   * origSize.Width),
                (int)(Top    * origSize.Height),
                (int)(Width  * origSize.Width),
                (int)(Height * origSize.Height)
                );
        }

        public void FlipHoriz()
        {
            //  [  l----w     ] orig state
            //  [     w----l  ] 1.0 - left state
            //  [     l----w  ] - width state
            Left = (1.0 - Left) - Width;
        }

        public void FlipVert()
        {
            Top = (1.0 - Top) - Height;
        }

        public void RotateCW()
        {
            double newWidth  = Height;
            double newHeight = Width;
            double newLeft   = 1.0 - (Top + Height);
            double newTop    = Left;

            Left    = newLeft;
            Top     = newTop;
            Width   = newWidth;
            Height  = newHeight;
        }

        public void RotateCCW()
        {
            double newWidth  = Height;
            double newHeight = Width;
            double newLeft   = Top;
            double newTop    = 1.0 - (Left + Width);

            Left    = newLeft;
            Top     = newTop;
            Width   = newWidth;
            Height  = newHeight;
        }

        public bool Same(FloatRect other)
        {
            return
                other.Left   == Left &&
                other.Top    == Top &&
                other.Width  == Width &&
                other.Height == Height ;
        }


    }

}
