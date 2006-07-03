using System.Xml.Serialization;
using System.Xml.Schema;

namespace FlickrNet
{
	[System.Serializable]
	public class Photosets
	{
		[XmlAttribute("cancreate", Form=XmlSchemaForm.Unqualified)]
		public int CanCreate;

		[XmlElement("photoset", Form=XmlSchemaForm.Unqualified)]
		public Photoset[] PhotosetCollection;
	}
	
	[System.Serializable]
	public class Photoset
	{
		[XmlAttribute("id", Form=XmlSchemaForm.Unqualified)]
		public string PhotosetId;

		[XmlAttribute("url", Form=XmlSchemaForm.Unqualified)]
		public string Url;

		[XmlAttribute("owner", Form=XmlSchemaForm.Unqualified)]
		public string OwnerId;

		[XmlAttribute("primary", Form=XmlSchemaForm.Unqualified)]
		public string PrimaryPhotoId;

		[XmlAttribute("secret", Form=XmlSchemaForm.Unqualified)]
		public string Secret;

		[XmlAttribute("server", Form=XmlSchemaForm.Unqualified)]
		public int Server;

		[XmlAttribute("photos", Form=XmlSchemaForm.Unqualified)]
		public int NumberOfPhotos;

		[XmlElement("title", Form=XmlSchemaForm.Unqualified)]
		public string Title;

		[XmlElement("description", Form=XmlSchemaForm.Unqualified)]
		public string Description;

		[XmlElement("photo", Form=XmlSchemaForm.Unqualified)]
		public Photo[] PhotoCollection = new Photo[0];
	}
}