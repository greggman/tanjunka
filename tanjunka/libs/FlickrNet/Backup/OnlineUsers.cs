using System.Xml.Serialization;
using System.Xml.Schema;

namespace FlickrNet
{
	[System.Serializable]
	public class Online
	{
		/// <remarks/>
		[XmlElement("user", Form=XmlSchemaForm.Unqualified)]
		public OnlineUser[] UserCollection;
	}

	[System.Serializable]
	public class OnlineUser
	{
		/// <remarks/>
		[XmlAttribute("nsid", Form=XmlSchemaForm.Unqualified)]
		public string UserId;
    
		/// <remarks/>
		[XmlAttribute("username", Form=XmlSchemaForm.Unqualified)]
		public string UserName;
    
		/// <remarks/>
		[XmlAttribute("online", Form=XmlSchemaForm.Unqualified)]
		public OnlineStatus IsOnline;

		/// <remarks/>
		[XmlText()]
		public string AwayDescription;
	}

	[System.Serializable]
	public enum OnlineStatus
	{
		[XmlEnum("1")]
		Away = 1,
		[XmlEnum("2")]
		Online = 2
	}

}