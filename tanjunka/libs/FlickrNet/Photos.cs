using System.Xml.Serialization;
using System.Xml.Schema;

namespace FlickrNet
{
	/// <remarks/>
	[System.Serializable]
	public class Photos 
	{
    
		/// <remarks/>
		[XmlElement("photo", Form=XmlSchemaForm.Unqualified)]
		public Photo[] PhotoCollection = new Photo[0];
    
		/// <remarks/>
		[XmlAttribute("page", Form=XmlSchemaForm.Unqualified)]
		public long PageNumber;
    
		/// <remarks/>
		[XmlAttribute("pages", Form=XmlSchemaForm.Unqualified)]
		public long TotalPages;
    
		/// <remarks/>
		[XmlAttribute("perpage", Form=XmlSchemaForm.Unqualified)]
		public long PhotosPerPage;
    
		/// <remarks/>
		[XmlAttribute("total", Form=XmlSchemaForm.Unqualified)]
		public long TotalPhotos;
	}

	/// <remarks/>
	[System.Serializable]
	public class Photo 
	{
    
		/// <remarks/>
		[XmlAttribute("id", Form=XmlSchemaForm.Unqualified)]
		public string PhotoId;
    
		/// <remarks/>
		[XmlAttribute("owner", Form=XmlSchemaForm.Unqualified)]
		public string UserId;
    
		/// <remarks/>
		[XmlAttribute("secret", Form=XmlSchemaForm.Unqualified)]
		public string Secret;
    
		/// <remarks/>
		[XmlAttribute("server", Form=XmlSchemaForm.Unqualified)]
		public string Server;
    
		/// <remarks/>
		[XmlAttribute("title", Form=XmlSchemaForm.Unqualified)]
		public string Title;
    
		/// <remarks/>
		[XmlAttribute("ispublic", Form=XmlSchemaForm.Unqualified)]
		public int IsPublic;
    
		/// <remarks/>
		[XmlAttribute("isfriend", Form=XmlSchemaForm.Unqualified)]
		public int IsFriend;
    
		/// <remarks/>
		[XmlAttribute("isfamily", Form=XmlSchemaForm.Unqualified)]
		public int IsFamily;

		/// <remarks/>
		[XmlAttribute("isprimary", Form=XmlSchemaForm.Unqualified)]
		public int IsPrimary;

		private const string photoUrl = "http://photos{0}.flickr.com/{1}_{2}{3}.jpg";

		[XmlIgnore()]
		public string SquareThumbnailUrl
		{
			get { return string.Format(photoUrl, Server, PhotoId, Secret, "_s"); }
		}

		[XmlIgnore()]
		public string ThumbnailUrl
		{
			get { return string.Format(photoUrl, Server, PhotoId, Secret, "_t"); }
		}

		[XmlIgnore()]
		public string SmallUrl
		{
			get { return string.Format(photoUrl, Server, PhotoId, Secret, "_m"); }
		}

		[XmlIgnore()]
		public string MediumUrl
		{
			get { return string.Format(photoUrl, Server, PhotoId, Secret, ""); }
		}

		[XmlIgnore()]
		public string LargeUrl
		{
			get { return string.Format(photoUrl, Server, PhotoId, Secret, "_b"); }
		}

	}

	[System.Serializable]
	public class PhotoPermissions
	{
		/// <remarks/>
		[XmlAttribute("id", Form=XmlSchemaForm.Unqualified)]
		public string PhotoId;

		/// <remarks/>
		[XmlAttribute("ispublic", Form=XmlSchemaForm.Unqualified)]
		public int IsPublic;
    
		/// <remarks/>
		[XmlAttribute("isfriend", Form=XmlSchemaForm.Unqualified)]
		public int IsFriend;
    
		/// <remarks/>
		[XmlAttribute("isfamily", Form=XmlSchemaForm.Unqualified)]
		public int IsFamily;

		/// <remarks/>
		[XmlAttribute("permcomment", Form=XmlSchemaForm.Unqualified)]
		public PermissionComment PermissionComment;

		/// <remarks/>
		[XmlAttribute("permaddmeta", Form=XmlSchemaForm.Unqualified)]
		public PermissionAddMeta PermissionAddMeta;

	}
}