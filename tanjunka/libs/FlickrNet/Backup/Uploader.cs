using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace FlickrNet
{
	/// <summary>
	/// Information returned by the UploadPicture url.
	/// </summary>
	[XmlRoot("uploader")]
	public class Uploader
	{
		/// <summary>
		/// The status of the upload, either "ok" or "fail".
		/// </summary>
		[XmlElement("status", Form=XmlSchemaForm.Unqualified)]
		public ResponseStatus Status;

		/// <summary>
		/// If the upload succeeded then this contains the id of the photo. Otherwise it will be zero.
		/// </summary>
		[XmlElement("photoid", Form=XmlSchemaForm.Unqualified)]
		public string PhotoId;

		/// <summary>
		/// If the upload failed then this contains the error code.
		/// </summary>
		[XmlElement("error", Form=XmlSchemaForm.Unqualified)]
		public int Code;

		/// <summary>
		/// If the upload failed then this contains the error description.
		/// </summary>
		[XmlElement("verbose", Form=XmlSchemaForm.Unqualified)]
		public string Message;

	}
}
