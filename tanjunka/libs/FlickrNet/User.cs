using System.Xml.Serialization;
using System.Xml.Schema;

namespace FlickrNet
{
	/// <summary>
	/// Contains details of a user
	/// </summary>
	[System.Serializable]
	public class User
	{
		/// <summary>
		/// The id of the user object.
		/// </summary>
		[XmlAttribute("nsid", Form=XmlSchemaForm.Unqualified)]
		public string UserId;

		/// <summary>
		/// The url for the user. Only available for objects returned 
		/// by <see cref="Flickr.UrlsGetUserPhotos"/> and <see cref="Flickr.UrlsGetUserProfile"/>.
		/// </summary>
		[XmlAttribute("url", Form=XmlSchemaForm.Unqualified)]
		public string Url;

		/// <summary>
		/// The Username of the selected user.
		/// Not available when returned by <see cref="Flickr.UrlsGetUserPhotos"/> and <see cref="Flickr.UrlsGetUserProfile"/>.
		/// </summary>
		[XmlElement("username")]
		public string UserName;
	}
}