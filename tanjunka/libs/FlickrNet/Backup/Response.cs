﻿using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace FlickrNet
{
	/// <summary>
	/// The root object returned by Flickr. Used with Xml Serialization to get the relevant object.
	/// </summary>
	[XmlRoot("rsp", Namespace="", IsNullable=false)]
	[Serializable]
	public class Response 
	{

		/// <remarks/>
		[XmlElement("blogs", Form=XmlSchemaForm.Unqualified)]
		public Blogs Blogs;

		/// <remarks/>
		[XmlElement("contacts", Form=XmlSchemaForm.Unqualified)]
		public Contacts Contacts;

		/// <remarks/>
		[XmlElement("photos", Form=XmlSchemaForm.Unqualified)]
		public Photos Photos;

		/// <remarks/>
		[XmlElement("category", Form=XmlSchemaForm.Unqualified)]
		public Category Category;

		/// <remarks/>
		[XmlElement("activegroups", Form=XmlSchemaForm.Unqualified)]
		public ActiveGroups ActiveGroups;

		/// <remarks/>
		[XmlElement("groups", Form=XmlSchemaForm.Unqualified)]
		public PoolGroups PoolGroups;

		/// <remarks/>
		[XmlElement("group", Form=XmlSchemaForm.Unqualified)]
		public GroupInfo GroupInfo;

		/// <remarks/>
		[XmlElement("photocounts", Form=XmlSchemaForm.Unqualified)]
		public PhotoCounts PhotoCounts;

		/// <remarks/>
		[XmlElement("photo", Form=XmlSchemaForm.Unqualified)]
		public PhotoInfo PhotoInfo;

		/// <remarks/>
		[XmlElement("who", Form=XmlSchemaForm.Unqualified)]
		public WhoInfo Who;

		/// <remarks/>
		[XmlElement("perms", Form=XmlSchemaForm.Unqualified)]
		public PhotoPermissions PhotoPermissions;

		/// <remarks/>
		[XmlElement("user", Form=XmlSchemaForm.Unqualified)]
		public User User;
    
		/// <remarks/>
		[XmlElement("person", Form=XmlSchemaForm.Unqualified)]
		public Person Person;
    
		/// <remarks/>
		[XmlElement("photoset", Form=XmlSchemaForm.Unqualified)]
		public Photoset Photoset;

		/// <remarks/>
		[XmlElement("photosets", Form=XmlSchemaForm.Unqualified)]
		public Photosets Photosets;

		/// <remarks/>
		[XmlElement("online", Form=XmlSchemaForm.Unqualified)]
		public Online OnlineUsers;
    
		/// <remarks/>
		[XmlElement("sizes", Form=XmlSchemaForm.Unqualified)]
		public Sizes Sizes;
    
		/// <remarks/>
		[XmlElement("licenses", Form=XmlSchemaForm.Unqualified)]
		public Licenses Licenses;
    
		/// <remarks/>
		[XmlAttribute("stat", Form=XmlSchemaForm.Unqualified)]
		public ResponseStatus Status;

		/// <summary>
		/// If an error occurs the Error property is populated with 
		/// a <see cref="ResponseError"/> instance.
		/// </summary>
		[XmlElement("err", Form=XmlSchemaForm.Unqualified)]
		public ResponseError Error;

		/// <summary>
		/// If using flickr.test.echo this contains all the other elements not covered above.
		/// </summary>
		/// <remarks>
		/// t is an array of <see cref="XmlElement"/> objects. Use the XmlElement Name and InnerXml properties
		/// to get the name and value of the returned property.
		/// </remarks>
		[XmlAnyElement()]
		public XmlElement[] AllElements;
	}

	/// <summary>
	/// If an error occurs then Flickr returns this object.
	/// </summary>
	[System.Serializable]
	public class ResponseError
	{
		/// <summary>
		/// The code or number of the error.
		/// </summary>
		/// <remarks>
		/// 100 - Invalid Api Key.
		/// 99  - User not logged in.
		/// Other codes are specific to a method.
		/// </remarks>
		[XmlAttribute("code", Form=XmlSchemaForm.Unqualified)]
		public int Code;

		/// <summary>
		/// The verbose message matching the error code.
		/// </summary>
		[XmlAttribute("msg", Form=XmlSchemaForm.Unqualified)]
		public string Message;
	}

	/// <summary>
	/// The status of the response, either ok or fail.
	/// </summary>
	public enum ResponseStatus
	{
		/// <summary>
		/// The response returns "ok" on a successful execution of the method.
		/// </summary>
		[XmlEnum("ok")]
		OK,
		/// <summary>
		/// The response returns "fail" if there is an error, such as invalid API key or login failure.
		/// </summary>
		[XmlEnum("fail")]
		Failed
	}
}