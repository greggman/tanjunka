using System.Xml.Serialization;
using System.Xml.Schema;

namespace FlickrNet
{
	/// <summary>
	/// Collection of <see cref="Size"/> items for a given photograph.
	/// </summary>
	[System.Serializable]
	public class Sizes
	{
		/// <summary>
		/// The size collection contains an array of <see cref="Size"/> items.
		/// </summary>
		[XmlElement("size", Form=XmlSchemaForm.Unqualified)]
		public Size[] SizeCollection = new Size[0];
	}

	/// <summary>
	/// Contains details about all the sizes available for a given photograph.
	/// </summary>
	[System.Serializable]
	public class Size
	{
		/// <summary>
		/// The label for the size, such as "Thumbnail", "Small", "Medium", "Large" and "Original".
		/// </summary>
		[XmlAttribute("label", Form=XmlSchemaForm.Unqualified)]
		public string Label;
    
        /// <summary>
        /// The width of the resulting image, in pixels
        /// </summary>
		[XmlAttribute("width", Form=XmlSchemaForm.Unqualified)]
		public int Width;
    
		/// <summary>
		/// The height of the resulting image, in pixels
		/// </summary>
		[XmlAttribute("height", Form=XmlSchemaForm.Unqualified)]
		public int Height;
    
		/// <summary>
		/// The source url of the image.
		/// </summary>
		[XmlAttribute("source", Form=XmlSchemaForm.Unqualified)]
		public string Source;
    
		/// <summary>
		/// The url to the photographs web page for this particular size.
		/// </summary>
		[XmlAttribute("url", Form=XmlSchemaForm.Unqualified)]
		public string Url;
	}
}