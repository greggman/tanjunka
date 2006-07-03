#region Using directives

using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Serialization;

#endregion

namespace Tanjunka
{
    // ----------------- picture ----------------------------------------------
    // this is a picture in Tanjunka
    //
    // It has:
    //
    //    1) an original non-compressed, non-resized, non-cropped, non-edited image
    //    2) an image stored as a temp file with all the current edits in place
    //       (resized, cropped, HSV adjusted, etc.
    //
    [Serializable]
    public class TJPicServiceInfo : ISerializable
    {
        private Dictionary<string,string>   m_data;

        public TJPicServiceInfo()
        {
            m_data = new Dictionary<string, string>();
        }

        protected TJPicServiceInfo(SerializationInfo info, StreamingContext context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            m_data  = genericInfo.GetValue<Dictionary<string, string>>("m_data");
        }

        public void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("m_data", m_data);
        }

        public string GetEntry (string dataname)
        {
            if (m_data.ContainsKey(dataname))
            {
                return m_data[dataname];
            }
            return "";
        }

        public void PutEntry (string dataname, string data)
        {
            m_data[dataname] = data;
        }
    }

    [Serializable]
    public class TJPagePicture : ISerializable
    {
        private const string jpeg_mimetype = "image/jpeg";

        private string originalImagePath;   // path to original (may no longer exist)
        private string tempImagePath;       // path to resized version
        private string customUploadName;    // name if "proxy_autogennames" is on
        private bool   b8bit;               // it's an 8bit image
        private int    versionNumber;   // this starts at 1, if the picture is edited it is incremented.  If the service's version number of this pic doesn't match then
                                        // it needs to be uploaded to that service

        private Size originalSize;
        private Size newSize;

        private Dictionary<string, TJPicServiceInfo>   m_perServiceData;
        private List<PhotoAction> m_actions;
        private List<PhotoAction> m_redoActions;

        private ActionResults m_actionResults;

        // DONT-SAVE
        private bool    originalIsTemp; // yes if we need to delete this
        private string newTempPath;     // this is the path of the image after loading
                                        // we need to remap the HTML from tempImagePath to this
                                        // next set tempImagePath to this

        public TJPagePicture(string imagePath)
        {
            originalImagePath = imagePath;
            tempImagePath  = Util.getTempFilename(Path.GetExtension(imagePath));
            newTempPath    = null;
            originalSize   = new Size(0, 0);
            b8bit          = false;
            versionNumber  = 1;
            originalIsTemp = false;
            m_perServiceData = new Dictionary<string, TJPicServiceInfo>();
            m_actions = new List<PhotoAction>();
            m_redoActions = new List<PhotoAction>();
            m_actionResults = new ActionResults();

            int picCount = UserSettings.G.GetIntEntry("global_pic_count") + 1;
            UserSettings.G.PutIntEntry("global_pic_count", picCount);
            customUploadName = String.Format("tanjun_{0:d4}{1:d2}{2:d2}_{3:d7}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, picCount);
        }

        // deserialize constructor
        protected TJPagePicture(SerializationInfo info, StreamingContext context)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            originalImagePath = genericInfo.GetValue<string>("m_originalImagePath");
            tempImagePath     = genericInfo.GetValue<string>("m_tempImagePath");
            customUploadName  = genericInfo.GetValue<string>("m_customUploadName");
            b8bit             = genericInfo.GetValue<bool>("m_8bit");
            versionNumber     = genericInfo.GetValue<int>("m_versionNumber");
            originalSize      = genericInfo.GetValue<Size>("m_originalSize");
            newSize           = genericInfo.GetValue<Size>("m_newSize");
            m_perServiceData  = genericInfo.GetValue<Dictionary<string,TJPicServiceInfo>>("m_perServiceData");

            m_actionResults = new ActionResults();
            {
                bool haveImage;
                haveImage = genericInfo.GetValue<bool>("m_haveOriginalImage");

                byte[] buf = genericInfo.GetValue<byte[]>("m_originalImage");
                originalImagePath = Path.Combine(Util.GetTempPath(), Path.GetFileName(originalImagePath));
                Util.writeFile(originalImagePath, buf);
                originalIsTemp = true;
            }
            {
                bool haveImage;
                haveImage = genericInfo.GetValue<bool>("m_haveTempImage");

                byte[] buf = genericInfo.GetValue<byte[]>("m_tempImage");
                newTempPath = Util.getTempFilename(Path.GetExtension(tempImagePath));
                Util.writeFile(newTempPath, buf);
            }

            if (EntryInfo.GetLoadVersion() >= 2)
            {
                m_actions = genericInfo.GetValue<List<PhotoAction>>("m_actions");
            }
            else
            {
                m_actions = new List<PhotoAction>();
            }

            m_redoActions = new List<PhotoAction>();
            ComputeActionResults();
        }

        public void GetObjectData(SerializationInfo info,StreamingContext ctx)
        {
            GenericSerializationInfo genericInfo = new GenericSerializationInfo(info);

            genericInfo.AddValue("m_originalImagePath", originalImagePath);
            genericInfo.AddValue("m_tempImagePath",     tempImagePath    );
            genericInfo.AddValue("m_customUploadName",  customUploadName );
            genericInfo.AddValue("m_8bit",              b8bit            );
            genericInfo.AddValue("m_versionNumber",     versionNumber    );
            genericInfo.AddValue("m_originalSize",      originalSize     );
            genericInfo.AddValue("m_newSize",           newSize          );
            genericInfo.AddValue("m_perServiceData",    m_perServiceData );

            // copy the images in as well
            {
                bool haveImage = true;
                genericInfo.AddValue("m_haveOriginalImage", haveImage);
                byte[] buf = Util.readFile(originalImagePath);
                genericInfo.AddValue("m_originalImage", buf);
            }
            {
                bool haveImage = true;
                genericInfo.AddValue("m_haveTempImage", haveImage);
                byte[] buf = Util.readFile(tempImagePath);
                genericInfo.AddValue("m_tempImage", buf);
            }

            // -- v2 --
            genericInfo.AddValue("m_actions", m_actions);
        }

        public static bool CanHandle(string filespec)
        {
            string ext = Path.GetExtension(filespec).ToLower();

            return (
                ext.CompareTo(".jpeg") == 0 ||
                ext.CompareTo(".jpg") == 0 ||
                ext.CompareTo(".png") == 0 ||
                ext.CompareTo(".gif") == 0);
        }

        public void BumpVersion()
        {
            versionNumber++;
        }

        public int GetVersion()
        {
            return versionNumber;
        }

        public bool Is8Bit()
        {
            return b8bit;
        }

        public bool Uncropped()
        {
            return m_actionResults.Uncropped();
        }

        public string GetTempPath()
        {
            return tempImagePath;
        }

        public string GetNewTempPath()
        {
            return newTempPath;
        }

        public void resetTempPath()
        {
            if (newTempPath != null)
            {
                tempImagePath = newTempPath;
                newTempPath = null;
            }
        }

        public string GetUploadName()
        {
            if (UserSettings.G.GetBoolEntry("prefs_autogennames"))
            {
                return customUploadName + Path.GetExtension(GetTempPath());
            }
            else
            {
                return Path.GetFileName(GetTempPath());
            }
        }

        public string GetOriginalPath()
        {
            return originalImagePath;
        }

        public int GetNewWidth()
        {
            return newSize.Width;
        }

        public int GetNewHeight()
        {
            return newSize.Height;
        }

        public int GetOrigWidth()
        {
            return originalSize.Width;
        }

        public int GetOrigHeight()
        {
            return originalSize.Height;
        }

        public void setNewSizeFromWidth(int newWidth)
        {
            newSize.Width  = newWidth;
            newSize.Height = newWidth * originalSize.Height / originalSize.Width;
        }

        public void setNewSizeFromHeight(int newHeight)
        {
            newSize.Height = newHeight;
            newSize.Width = newHeight * originalSize.Width / originalSize.Height;
        }

        public bool SameRect(ActionResults otherAR)
        {
            return m_actionResults.SameRect(otherAR);
        }

        public bool Same(ActionResults otherAR)
        {
            return m_actionResults.Same(otherAR);
        }

        public void boundToSize(int maxWidth, int maxHeight)
        {
            if (newSize.Width > maxWidth)
            {
                setNewSizeFromWidth(maxWidth);
            }
            if (newSize.Height > maxHeight)
            {
                setNewSizeFromHeight(maxHeight);
            }
        }

        private bool resizeImage(Image mg, bool bForceNewImage)
        {
            Rectangle editRect = m_actionResults.GetRotatedIntRect(mg.Size);

            b8bit = (mg.PixelFormat == PixelFormat.Format1bppIndexed ||
                     mg.PixelFormat == PixelFormat.Format4bppIndexed ||
                     mg.PixelFormat == PixelFormat.Format8bppIndexed ||
                     mg.PixelFormat == PixelFormat.Indexed ||
                     mg.Palette.Entries.Length != 0);

            //if (b8bit || (newSize.Width == mg.Width && newSize.Height == mg.Height))
            if (!bForceNewImage && newSize.Width == mg.Width && newSize.Height == mg.Height)
            {
                Util.deleteFileIfExists(tempImagePath);
                tempImagePath = Util.changeExtension(tempImagePath, Path.GetExtension(originalImagePath));
                Util.copyFile (originalImagePath, tempImagePath);
            }
            else
            {
                Util.deleteFileIfExists(tempImagePath);
                tempImagePath = Util.changeExtension(tempImagePath, ".jpg");

                Bitmap bp = new Bitmap(newSize.Width, newSize.Height);
                Graphics g = Graphics.FromImage(bp);

                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                Rectangle rect = new Rectangle(0, 0, newSize.Width, newSize.Height);
                // Draw the old image on to the new image using the graphics object:
                {
                    ImageAttributes imageAttr = new ImageAttributes();
                    imageAttr.SetColorMatrix(m_actionResults.GetColorMatrix());
                    imageAttr.SetGamma(m_actionResults.GetImageGamma());

                    mg.RotateFlip(m_actionResults.RotateFlip);

                    // g.DrawImage(mg, rect, 0, 0, mg.Width, mg.Height, GraphicsUnit.Pixel, imageAttr);
                    g.DrawImage(mg, rect, editRect.Left, editRect.Top, editRect.Width, editRect.Height, GraphicsUnit.Pixel, imageAttr);
                }

                // The image's new dimensions are contained in a bitmap object
                // (bp), which is stored in memory. At this point, you can copy
                // the metadata from the old image into the new image:

                foreach (PropertyItem pItem in mg.PropertyItems)
                {
                    bp.SetPropertyItem(pItem);
                }

                // find jpeg
                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo codec = null;
                for (int i = 0; i < codecs.Length; i++)
                {
                    if (codecs[i].MimeType.Equals(jpeg_mimetype))
                    {
                        codec = codecs[i];
                    }
                }

                // TODO: extension needs to be same as incoming file
                string tfile = tempImagePath;

                //if you find a codec for jpeg save image using codec
                //specify color depth and image quality

                if (codec != null)
                {
                    Encoder encoderInstance = null;
                    EncoderParameter encoderParameterInstance = null;
                    EncoderParameters encoderParametersInstance = null;

                    encoderInstance = Encoder.Quality;
                    encoderParametersInstance = new EncoderParameters(2);
                    encoderParameterInstance = new EncoderParameter(encoderInstance, 80L);
                    encoderParametersInstance.Param[0] = encoderParameterInstance;
                    encoderInstance = Encoder.ColorDepth;
                    encoderParameterInstance = new EncoderParameter(encoderInstance, 24L);
                    encoderParametersInstance.Param[1] = encoderParameterInstance;
                    bp.Save(tfile, codec, encoderParametersInstance);
                }
                else
                {
                    //no codec found, save image as an ordinary image
                    bp.Save(tfile, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                bp.Dispose();
                g.Dispose();
            }
            return true;
        }

        public Bitmap GenerateEditImage()
        {
            Image mg = Image.FromFile(originalImagePath, true);

            Rectangle editRect = m_actionResults.GetRotatedIntRect(mg.Size);

            Bitmap bp = new Bitmap(editRect.Width, editRect.Height);
            Graphics g = Graphics.FromImage(bp);

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            mg.RotateFlip(m_actionResults.RotateFlip);

            Rectangle rect = new Rectangle(0, 0, editRect.Width, editRect.Height);
            g.DrawImage(mg, rect, editRect.Left, editRect.Top, editRect.Width, editRect.Height, GraphicsUnit.Pixel);
            g.Dispose();

            return bp;
        }

        public bool resizeTempFileToFit (int maxWidth, int maxHeight, bool bForceNewImage)
        {
            // read and process image
            Image mg = Image.FromFile(originalImagePath, true);
            Rectangle editRect = m_actionResults.GetRotatedIntRect(mg.Size);

            if (originalSize.Width == 0 || bForceNewImage)
            {
                originalSize = editRect.Size;
                newSize = editRect.Size;

                boundToSize(maxWidth, maxHeight);
            }

            bool result = resizeImage(mg, bForceNewImage);
            mg.Dispose();

            return result;
        }

        public bool resizeTempFile(int newWidth, int newHeight)
        {
            // only resize if it's a new size
            if (newWidth != newSize.Width || newHeight != newSize.Height)
            {
                // read and process image
                Image mg = Image.FromFile(originalImagePath, true);

                newSize.Width  = newWidth;
                newSize.Height = newHeight;

                bool result = resizeImage(mg, false);
                mg.Dispose();
                return result;
            }
            return true;
        }

        public void deleteTempFile()
        {
            if (newTempPath != null)
            {
                Util.deleteFileIfExists(newTempPath);
            }
            else
            {
                Util.deleteFileIfExists(tempImagePath);
            }
            if (originalIsTemp)
            {
                Util.deleteFileIfExists(originalImagePath);
            }
        }

        public static string MakeJPEG(string filename)
        {
            // read and process image
            Image mg = Image.FromFile(filename, true);
            string newFilename = CreateTempBitmap(mg as Bitmap);
            mg.Dispose();

            return newFilename;
        }

        public static string CreateTempBitmap (Bitmap bm)
        {
            string tfile = Util.getTempFilename(".jpg");
            // find jpeg
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo codec = null;
            for (int i = 0; i < codecs.Length; i++)
            {
                if (codecs[i].MimeType.Equals(jpeg_mimetype))
                {
                    codec = codecs[i];
                }
            }

            if (codec != null)
            {
                Encoder encoderInstance = null;
                EncoderParameter encoderParameterInstance = null;
                EncoderParameters encoderParametersInstance = null;

                encoderInstance = Encoder.Quality;
                encoderParametersInstance = new EncoderParameters(2);
                encoderParameterInstance = new EncoderParameter(encoderInstance, 80L);
                encoderParametersInstance.Param[0] = encoderParameterInstance;
                encoderInstance = Encoder.ColorDepth;
                encoderParameterInstance = new EncoderParameter(encoderInstance, 24L);
                encoderParametersInstance.Param[1] = encoderParameterInstance;
                bm.Save(tfile, codec, encoderParametersInstance);
            }
            else
            {
                //no codec found, save image as an ordinary image
                bm.Save(tfile, System.Drawing.Imaging.ImageFormat.Jpeg);
            }

            return tfile;
        }

        public TJPicServiceInfo GetPerServiceInfo(BlogPhotoService bps)
        {
            TJPicServiceInfo info;

            if (m_perServiceData.ContainsKey(bps.GetID()))
            {
                info = m_perServiceData[bps.GetID()];
            }
            else
            {
                info = new TJPicServiceInfo();
                m_perServiceData[bps.GetID()] = info;
            }
            return info;
        }

        public string GetPhotoEntry (BlogPhotoService bps, string dataname)
        {
            TJPicServiceInfo info = GetPerServiceInfo(bps);

            return info.GetEntry(dataname);
        }

        public void PutPhotoEntry (BlogPhotoService bps, string dataname, string data)
        {
            TJPicServiceInfo info = GetPerServiceInfo(bps);

            info.PutEntry(dataname, data);
        }

        public int GetIntPhotoEntry (BlogPhotoService bps, string dataname)
        {
            int value = 0;
            string valueStr = GetPhotoEntry (bps, dataname);
            if (valueStr.Length > 0)
            {
                value = int.Parse(valueStr);
            }
            return value;
        }

        public void PutIntPhotoEntry (BlogPhotoService bps, string dataname, int value)
        {
            PutPhotoEntry(bps, dataname, value.ToString());
        }

        public bool GetBoolPhotoEntry (BlogPhotoService bps, string dataname)
        {
            return GetIntPhotoEntry (bps, dataname) != 0;
        }

        public void PutBoolPhotoEntry (BlogPhotoService bps, string dataname, bool value)
        {
            PutIntPhotoEntry(bps, dataname, value ? 1 : 0);
        }


        // --------------- actions -----------------------

        public void RealAddAction (PhotoAction action)
        {
            // effect the results
            action.EffectResults(m_actionResults);
            // add it to the list
            m_actions.Add(action);
        }

        public void AddAction (PhotoAction action)
        {
            RealAddAction(action);
            // kill all the redos
            m_redoActions.Clear();
        }

        // returns undone action which contains info for reseting controls
        public bool UndoAction ()
        {
            if (m_actions.Count > 0)
            {
                PhotoAction ac = m_actions[m_actions.Count - 1];
                m_actions.RemoveAt(m_actions.Count - 1);
                m_redoActions.Add(ac);

                ComputeActionResults();

                return true;
            }

            return false;
        }

        public bool RedoAction()
        {
            if (m_redoActions.Count > 0)
            {
                PhotoAction ac = m_redoActions[m_redoActions.Count - 1];
                m_redoActions.RemoveAt(m_redoActions.Count - 1);
                RealAddAction(ac);

                return true;
            }

            return false;
        }

        public PhotoAction TopRedoAction()
        {
            if (m_redoActions.Count > 0)
            {
                return m_redoActions[m_redoActions.Count - 1];
            }

            return null;
        }

        public bool HaveActions()
        {
            return m_actions.Count != 0;
        }

        public bool HaveRedoActions()
        {
            return m_redoActions.Count != 0;
        }

        public void ClearActions()
        {
            // need a "reset" action
        }

        private void ComputeActionResults()
        {
            m_actionResults.Reset();
            foreach (PhotoAction ac in m_actions)
            {
                ac.EffectResults(m_actionResults);
            }
        }

        public void CopyResults (ActionResults dst)
        {
            m_actionResults.CopyTo(dst);
        }

    }

}