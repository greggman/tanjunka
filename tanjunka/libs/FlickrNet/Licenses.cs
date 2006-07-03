using System;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace FlickrNet
{
	[System.Serializable]
	public class Licenses
	{
		/// <summary>A collection of available licenses.</summary>
		/// <remarks/>
		[XmlElement("license", Form=XmlSchemaForm.Unqualified)]
		public License[] LicenseCollection;
    
	}

	[System.Serializable]
	public class License
	{
		/// <summary>The ID of the license. Used by <see cref="Flickr.PhotosGetInfo"/>.</summary>
		/// <remarks/>
		[XmlAttribute("id", Form=XmlSchemaForm.Unqualified)]
		public int LicenseId;

		/// <summary>The name of the license.</summary>
		/// <remarks/>
		[XmlAttribute("name", Form=XmlSchemaForm.Unqualified)]
		public string LicenseName;

		/// <summary>The URL for the license text.</summary>
		/// <remarks/>
		[XmlAttribute("url", Form=XmlSchemaForm.Unqualified)]
		public string LicenseUrl;

	}
}
