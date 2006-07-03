using System;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace FlickrNet
{
	/// <summary>
	/// Contains a list of active <see cref="Group"/> items.
	/// </summary>
	/// <remarks>
	/// This information is taken from FlickrLive.
	/// </remarks>
	[System.Serializable]
	public class ActiveGroups
	{
		/// <remarks/>
		[XmlElement("group", Form=XmlSchemaForm.Unqualified)]
		public Group[] GroupsCollection;
	}

	/// <summary>
	/// Provides details of a particular group.
	/// </summary>
	/// <remarks>Used by <see cref="Flickr.GroupsGetActiveList"/> and <see cref="Flickr.GroupsBrowse"/>.</remarks>
	[System.Serializable]
	public class Group
	{
		/// <summary>
		/// The id of the group.
		/// </summary>
		[XmlAttribute("nsid", Form=XmlSchemaForm.Unqualified)]
		public string GroupId;
    
		/// <summary>
		/// The name of the group
		/// </summary>
		[XmlAttribute("name", Form=XmlSchemaForm.Unqualified)]
		public string GroupName;

		/// <summary>
		/// The number of memebers of the group.
		/// </summary>
		[XmlAttribute("members", Form=XmlSchemaForm.Unqualified)]
		public long Members;
	
		/// <summary>
		/// The number of users online in FlickrLive for this group.
		/// </summary>
		[XmlAttribute("online", Form=XmlSchemaForm.Unqualified)]
		public int NumberOnline;
	
		/// <summary>
		/// The chat id of the chat room for this group.
		/// </summary>
		[XmlAttribute("chatnsid", Form=XmlSchemaForm.Unqualified)]
		public string ChatId;
	
		/// <summary>
		/// The number of users in the chat room in FlickrLive for this group.
		/// </summary>
		[XmlAttribute("inchat", Form=XmlSchemaForm.Unqualified)]
		public int NumberInChat;
	}

	/// <summary>
	/// Provides details of a particular group.
	/// </summary>
	/// <remarks>
	/// Used by the Url methods and <see cref="Flickr.GroupsGetInfo"/> method.
	/// The reason for a <see cref="Group"/> and <see cref="GroupInfo"/> are due to xml serialization
	/// incompatabilities.
	/// </remarks>
	[System.Serializable]
	public class GroupInfo
	{
		/// <remarks/>
		[XmlAttribute("id", Form=XmlSchemaForm.Unqualified)]
		public string GroupId;
    
		/// <remarks/>
		[XmlAttribute("url", Form=XmlSchemaForm.Unqualified)]
		public string Url;

		/// <remarks/>
		[XmlElement("name", Form=XmlSchemaForm.Unqualified)]
		public string GroupName;

		/// <remarks/>
		[XmlElement("members", Form=XmlSchemaForm.Unqualified)]
		public long Members;
	
		/// <remarks/>
		[XmlElement("online", Form=XmlSchemaForm.Unqualified)]
		public int NumberOnline;
	
		/// <remarks/>
		[XmlElement("privacy", Form=XmlSchemaForm.Unqualified)]
		public PoolPrivacy Privacy;
	
		/// <remarks/>
		[XmlElement("chatnsid", Form=XmlSchemaForm.Unqualified)]
		public string ChatId;
	
		/// <remarks/>
		[XmlElement("chatcount", Form=XmlSchemaForm.Unqualified)]
		public int NumberInChat;

		public static implicit operator Group( GroupInfo gi )	
		{
			Group g = new Group();
			g.GroupId = gi.GroupId;
			g.GroupName = gi.GroupName;
			g.Members = gi.Members;
			g.NumberOnline = gi.NumberOnline;
			g.ChatId = gi.ChatId;
			g.NumberInChat = gi.NumberInChat;

			return g;
		}

		public static explicit operator GroupInfo( Group g )	
		{
			GroupInfo gi = new GroupInfo();
			gi.GroupId = g.GroupId;
			gi.GroupName = g.GroupName;
			gi.Members = g.Members;
			gi.NumberOnline = g.NumberOnline;
			gi.NumberInChat = g.NumberInChat;
			gi.ChatId = g.ChatId;

			return gi;
		}
	}

	[System.Serializable]
	public class PoolGroups
	{
		/// <remarks/>
		[XmlElement("group", Form=XmlSchemaForm.Unqualified)]
		public PoolInfo[] GroupsCollection;
	}

	[System.Serializable]
	public class PoolInfo
	{
		/// <remarks/>
		[XmlAttribute("id", Form=XmlSchemaForm.Unqualified)]
		public string GroupId;
    
		/// <remarks/>
		[XmlAttribute("name", Form=XmlSchemaForm.Unqualified)]
		public string GroupName;

		/// <remarks/>
		[XmlAttribute("admin", Form=XmlSchemaForm.Unqualified)]
		public int IsAdmin;
	
		/// <remarks/>
		[XmlAttribute("privacy", Form=XmlSchemaForm.Unqualified)]
		public PoolPrivacy privacy;
	
		/// <remarks/>
		[XmlAttribute("photos", Form=XmlSchemaForm.Unqualified)]
		public long PhotoCount;
	}

	[System.Serializable]
	public enum PoolPrivacy
	{
		[XmlEnum("1")]
		Private = 1,
		[XmlEnum("2")]
		InviteOnlyPublic = 2,
		[XmlEnum("3")]
		OpenPublic = 3
	}

}
