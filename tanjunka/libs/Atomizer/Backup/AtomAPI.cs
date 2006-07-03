using System;
using System.Xml;
using System.Net;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Xml.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Atomizer
{
	#region TestedEndpoints
	// http://www.blogger.com/atom/
	// http://www.typepad.com/t/atom/weblog
	// http://www.typepad.com/t/atom/gallery
	// http://www.typepad.com/t/atom/lists
	#endregion

	/// <summary>
	/// Atom: Implements a C# interface to the ATOM API set.
	/// </summary>
	public class Atom
	{
		#region Class data

		protected Uri m_endPointURL;
		protected string m_userName;
		protected string m_password;
		protected generatorType m_generator;
		protected WebProxy m_proxy;
		protected AtomDebug dbg = null;

		private enum postType
		{
			List,
			Entry,
			Edit,
			Delete,
			Feed
		}
		#endregion

		/// <summary>
		/// Atom Constructor: Use CREATE method instead
		/// </summary>
		protected Atom(Uri endPointURL, generatorType generator, string username, string password)
		{
			m_endPointURL = endPointURL;
			m_generator = generator;
			m_userName = username;
			m_password = password;
			m_proxy = null;

			dbg = new AtomDebug();
			dbg.Out("Creating new Atom base object");
		}

		public static Atom Create(Uri endPointURL, generatorType generator, string username, string password)
		{
			Atom retAPI = null;

			if (endPointURL.ToString().IndexOf("blogger.com") != -1)
			{
				retAPI = new Blogger(endPointURL, generator, username, password);
			}
			else if (endPointURL.ToString().IndexOf("typepad.com") != -1)
			{
				retAPI = new TypePad(endPointURL, generator, username, password);
			}
			else
			{
				retAPI = new Atom(endPointURL, generator, username, password);
			}

			return retAPI;
		}

		public generatorType Generator
		{
			get
			{
				return m_generator;
			}
			set
			{
				m_generator = value;
			}
		}

		public string UserName
		{
			get
			{
				return m_userName;
			}
			set
			{
				m_userName = value;
			}
		}

		public string Password
		{
			get
			{
				return m_password;
			}
			set
			{
				m_password = value;
			}
		}

		public WebProxy Proxy
		{
			get
			{
				return m_proxy;
			}
			set
			{
				m_proxy = value;
			}
		}

		/// <summary>
		/// Atom::GetServices: Returns an array containing the user's service endpoints.
		/// </summary>
		public virtual service[] GetServices()
		{
			dbg.TraceIn();

			service[] Services = null;

			XmlTextReader xmlReader = PostToATOMAPI(postType.List, null, null, null, null);
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(xmlReader);

			//get struct list
			XmlNodeList xmlNodes = xmlDoc.DocumentElement.ChildNodes;

			Services = new service[xmlNodes.Count];
			int x = 0;

			//get member list per struct
			foreach (XmlNode xmlNode in xmlNodes)
			{
				string url = "";
				string blogName = "";
				serviceType type = serviceType.unknown;

				foreach (XmlAttribute xmlAttr in xmlNode.Attributes)
				{
					if(xmlAttr.Name.Equals("href"))
					{
						url = xmlAttr.Value;
					}

					if (xmlAttr.Name.Equals("title"))
					{
						blogName = xmlAttr.Value;
					}

					if(xmlAttr.Name.Equals("rel"))
					{
						type = GetServiceFromString(xmlAttr.Value);
					}
				}

				Services[x] = new service();
				Services[x].srvType = type;
				Services[x].name = blogName;
				Services[x].postURL = url;
				x++;
			}

			dbg.TraceOut();

			return Services;
		}
		
		/// <summary>
		/// Atom::PostBlogEntry: Sends a blog entry to the specified blog
		/// </summary>
		public entryType PostBlogEntry(string postURL, string title, string content, string category)
		{
			dbg.TraceIn();
			
			XmlTextReader xmlText = PostToATOMAPI(postType.Entry, postURL, title, content, category);
			
			dbg.TraceOut();

			return DeserializeToEntry(xmlText);
		}


		/// <summary>
		/// Atom::EditBlogEntry: Edits the specified entry
		/// Use the service.edit URL to do the edit
		/// </summary>
		public entryType EditBlogEntry(string editURL, string title, string content, string category)
		{
			dbg.TraceIn();

			XmlTextReader xmlText = PostToATOMAPI(postType.Edit, editURL, title, content, category);
			XmlSerializer serializer = new XmlSerializer(typeof(entryType));
			serializer.UnknownElement += new XmlElementEventHandler(Serializer_contentType);

			entryType blogEntry = null;

			try
			{
				blogEntry = (entryType)serializer.Deserialize(xmlText);
			}
			catch
			{
				//maybe it's a blogger type with no ns
				//although very blogger-specific, leave this in the base class,
				//as they have said they will fix it in the next rev
				if (this is Blogger)
				{
					Blogger bl = this as Blogger;
					blogEntry = bl.ConvertToBloggerEntry(xmlText);
				}
			}
			
			//figure out the type based on the link.rel
			foreach (linkType link in blogEntry.links)
			{
				link.srvType = GetServiceFromString(link.rel);
			}
			
			dbg.TraceOut();

			return blogEntry;
		}


		/// <summary>
		/// Atom::GetFeed: Downloads a feed from a service.feed
		/// </summary>
		public feedType GetFeed(string feedURL)
		{
			dbg.TraceIn();

			XmlTextReader xmlText = PostToATOMAPI(postType.Feed, feedURL, null, null, null);
			XmlSerializer serializer = new XmlSerializer(typeof(feedType));
			serializer.UnknownElement += new XmlElementEventHandler(Serializer_contentType);

			feedType feedList = null;

			try
			{
				feedList = (feedType)serializer.Deserialize(xmlText);

				//fix up the links in the feed header
				foreach (linkType link in feedList.links)
				{
					link.srvType = GetServiceFromString(link.rel);
				}

				//fix up the links in each entry
				foreach (entryType entry in feedList.entries)
				{
					foreach (linkType linkE in entry.links)
					{
						linkE.srvType = GetServiceFromString(linkE.rel);
					}
				}
			}
			catch
			{
			}
			
			dbg.TraceOut();

			return feedList;
		}


		/// <summary>
		/// Atom::DeleteBlogEntry: Deletes the specified entry
		/// Use the service.edit URL to do the delete
		/// </summary>
		public void DeleteBlogEntry(string deleteURL)
		{
			dbg.TraceIn();

			PostToATOMAPI(postType.Delete, deleteURL, null, null, null);

			dbg.TraceOut();
		}

		public entryType DeserializeToEntry(XmlTextReader xmlText)
		{
			dbg.TraceIn();

			XmlSerializer serializer = new XmlSerializer(typeof(entryType));
			serializer.UnknownElement += new XmlElementEventHandler(Serializer_contentType);

			entryType blogEntry = null;

			try
			{
				blogEntry = (entryType)serializer.Deserialize(xmlText);
			}
			catch
			{
				//maybe it's a blogger type with no ns
				//although very blogger-specific, leave this in the base class,
				//as they have said they will fix it in the next rev
				if (this is Blogger)
				{
					Blogger bl = this as Blogger;
					blogEntry = bl.ConvertToBloggerEntry(xmlText);
				}
			}
			
			//figure out the type based on the link.rel
			foreach (linkType link in blogEntry.links)
			{
				link.srvType = GetServiceFromString(link.rel);
			}
			
			dbg.TraceOut();

			return blogEntry;
		}

		/// <summary>
		/// GetServiceFromString: Map a "link.rel" like "service.XXXX" to an enum
		/// </summary>
		private serviceType GetServiceFromString(string rel)
		{
			dbg.TraceIn();

			serviceType type = serviceType.unknown;

			if (rel.Equals("service.post"))
			{
				type = serviceType.post;
			}
			else if (rel.Equals("service.feed"))
			{
				type = serviceType.feed;
			}
			else if (rel.Equals("service.upload"))
			{
				type = serviceType.upload;
			}
			else if (rel.Equals("service.categories"))
			{
				type = serviceType.categories;
			}
			else if (rel.Equals("service.edit"))
			{
				type = serviceType.edit;
			}
			else if (rel.Equals("alternate"))
			{
				type = serviceType.alternate;
			}
			else if (rel.Equals("prev"))
			{
				type = serviceType.prevFeed;
			}
			else if (rel.Equals("next"))
			{
				type = serviceType.nextFeed;
			}
			else if (rel.Equals("start"))
			{
				type = serviceType.firstFeed;
			}

			dbg.TraceOut();

			return type;
		}

		/// <summary>
		/// CreateATOMXML: Helper function to create an entry
		/// </summary>
		private void CreateATOMXML(XmlTextWriter writer, string title, string content, string category)
		{
			dbg.TraceIn();

			writer.Formatting = Formatting.Indented;
			writer.Indentation = 1;
			writer.IndentChar = '\t';

			//start an xml-rpc call
			writer.WriteStartDocument();

			writer.WriteStartElement("entry");
			
			//entry attributes
			writer.WriteAttributeString("version", "0.3");
			writer.WriteAttributeString("xmlns", "http://purl.org/atom/ns#");

			//if we have a category, we need to add another namespace
			if ((category != null) && (category.Length > 0))
			{
				writer.WriteAttributeString("xmlns:dc", "http://purl.org/dc/elements/1.1/");
			}
			
			Assembly assem = Assembly.GetExecutingAssembly();
			object[] objs = assem.GetCustomAttributes(typeof(AssemblyFileVersionAttribute),false);

			string version = "1.0";
			foreach (object obj in objs)
			{
				if (obj is AssemblyFileVersionAttribute)
				{
					AssemblyFileVersionAttribute fv = (AssemblyFileVersionAttribute)obj;
					version = fv.Version;
				}
			}

			objs = assem.GetCustomAttributes(typeof(AssemblyProductAttribute),false);
			string product = "";
			foreach (object obj in objs)
			{
				if (obj is AssemblyProductAttribute)
				{
					AssemblyProductAttribute pro = (AssemblyProductAttribute)obj;
					product = pro.Product;
				}
			}

			//generator element
			writer.WriteStartElement("generator");
			writer.WriteAttributeString("url", m_generator.url);
			writer.WriteAttributeString("version", m_generator.version + " (" + version + ")");
			writer.WriteString(m_generator.Value + " (" + product + ")");
			writer.WriteEndElement();

			//title element
			//you don't have to have a title -- pass in "" to skip this
			if ((title != null) && (title.Length > 0))
			{
				writer.WriteStartElement("title");
				writer.WriteAttributeString("mode", "escaped");
				writer.WriteAttributeString("type", "text/html");
				writer.WriteString(title);
				writer.WriteEndElement();
			}

			//category element
			//you don't have to have a category -- pass in "" to skip this
			if ((category != null) && (category.Length > 0))
			{
				writer.WriteStartElement("dc:subject");
				writer.WriteAttributeString("mode", "escaped");
				writer.WriteString(category);
				writer.WriteEndElement();
			}

			writer.WriteElementString("issued", 
				DateTime.UtcNow.ToString("u",CultureInfo.InvariantCulture).Replace(' ','T'));

			//content element
			writer.WriteStartElement("content");
			writer.WriteAttributeString("mode", "escaped");
			writer.WriteAttributeString("type", "text/html");
			writer.WriteString(content);
			writer.WriteEndElement();

			writer.WriteEndElement(); //close out the doc

			//ker-flush!
			writer.Flush();

			dbg.TraceOut();
		}

		/// <summary>
		/// GenerateXWSSEHeader: Helper function to do XWSSE stuff
		/// </summary>
		public void GenerateXWSSEHeader(WebRequest webreq, string username, string password)
		{
			dbg.TraceIn();

			string Nonce = new Random().Next().ToString(CultureInfo.InvariantCulture);
			string Created=DateTime.UtcNow.ToString("u",CultureInfo.InvariantCulture).Replace(' ','T');
   
			// Fill in the password
			SHA1 md = new SHA1CryptoServiceProvider();
			string v = Nonce + Created + password;
			byte[] digest = md.ComputeHash(Encoding.Default.GetBytes(v));
			string Password64 = Convert.ToBase64String(digest);
			
			webreq.Headers.Add("X-WSSE", "UsernameToken Username=\""+
				username+"\", "+
				"PasswordDigest=\""+
				Password64+"\", "+
				"Nonce=\""+
				Convert.ToBase64String(Encoding.Default.GetBytes(Nonce))+"\", "+
				"Created=\""+
				Created+"\"");
		
			dbg.TraceOut();
		}
		

		/// <summary>
		/// PosttoATOMAPI: Helper function to do the real work of making an ATOM request
		/// </summary>
		private XmlTextReader PostToATOMAPI(postType pt, string postURL, string title, string content, string category)
		{
			dbg.TraceIn();

			HttpWebRequest webreq = null;
			XmlTextWriter writer = null;
			MemoryStream xmlstream = null;

			string url = m_endPointURL.ToString();

			if (!pt.Equals(postType.List))
			{
				url = postURL;
			}

			webreq = (HttpWebRequest)HttpWebRequest.Create(url);

			if (m_proxy != null)
			{
				webreq.Proxy = m_proxy;
			}

			//build the get/post request by hand.
			webreq.UserAgent     = m_generator.Value;

			if (pt.Equals(postType.Entry)
				|| pt.Equals(postType.Edit))
			{
				xmlstream = new MemoryStream();
				writer = new XmlTextWriter(xmlstream, Encoding.UTF8);

				CreateATOMXML(writer, title, content, category);

				webreq.ContentType   = "application/xml";
				webreq.ContentLength = xmlstream.Length;
				webreq.Method		 = "POST";

				if (pt.Equals(postType.Edit))
				{
					webreq.Method        = "PUT";
					webreq.AllowAutoRedirect = false;
				}
			}
			else if (pt.Equals(postType.Delete))
			{
				webreq.Method		 = "DELETE";
			}
			else
			{
				webreq.Method        = "GET";
			}

			//set up the password encryption
			GenerateXWSSEHeader(webreq, m_userName, m_password);

			//in case we need to, we'll do regular http auth types, too ... thanks, .NET -- this is pretty easy
			webreq.Credentials = new System.Net.NetworkCredential(m_userName, m_password);

			if (pt.Equals(postType.Entry) || pt.Equals(postType.Edit))
			{
				Stream newStream = webreq.GetRequestStream();
				newStream.Write(xmlstream.GetBuffer(), 0, (int)xmlstream.Length);
				newStream.Flush();
				newStream.Close();
				writer.Close();
			}
		
			HttpWebResponse webresp = null;
			bool fDeleted = false;

			try
			{
				webresp = (HttpWebResponse)webreq.GetResponse();
			}
			catch(WebException ex)
			{
				//spec says OK instead of Gone, but I guess this is OK
				if (pt.Equals(postType.Delete))
				{
					if (((System.Net.HttpWebResponse)(((System.Net.WebResponse)(ex.Response)))).StatusCode != HttpStatusCode.Gone)
					{
						throw(ex);
					}

					fDeleted = true;
				}
				else
				{
					throw(ex);
				}
			}

			if (!fDeleted)
			{
				HttpStatusCode expectedCode = HttpStatusCode.OK;
				
				switch (pt)
				{
						//workaround: blogger.com returns "301: Moved Perm" on an edit instead of "ResetContent"
					case postType.Edit :
						expectedCode = HttpStatusCode.MovedPermanently;
						break;

					case postType.Entry : 
						expectedCode = HttpStatusCode.Created;
						break;

					case postType.Delete :
						expectedCode = HttpStatusCode.NoContent;
						break;
				}

				if ((webresp.StatusCode != expectedCode)
					&&
					(webresp.StatusCode != HttpStatusCode.OK)) //assume "OK" is always good
				{
					webresp.Close();
					throw(new WebException(webresp.StatusDescription));
				}
			}

			XmlTextReader xmlReader = null;

			if (!pt.Equals(postType.Delete))
			{
				Stream xmlResp = webresp.GetResponseStream();
				xmlReader = new XmlTextReader(xmlResp);
			}

			dbg.TraceOut();

			return xmlReader;
		}
		
		private void ElementToContents(XmlElement e, contentType ct)
		{
			dbg.TraceIn();

			ct.Text = e.InnerXml;

			XmlAttributeCollection attrs = e.Attributes;

			foreach (XmlAttribute attr in attrs)
			{
				if (attr.Name.Equals("lang"))
				{
					ct.lang = attr.Value;
				}
				else if (attr.Name.Equals("mode"))
				{
					ct.mode = attr.Value;
				}
				else if (attr.Name.Equals("type"))
				{
					ct.type = attr.Value;
				}
			}

			dbg.TraceOut();
		}

		protected void Serializer_contentType(object sender, XmlElementEventArgs e)
		{
			dbg.TraceIn();

			bool fSummary = (e.Element.Name == "summary");
			bool fContent = (e.Element.Name == "content");

			if ( fSummary || fContent)
			{
				if (e.ObjectBeingDeserialized is entryType)
				{
					entryType entry = (entryType)e.ObjectBeingDeserialized;

					if (fSummary)
					{
						entry.summaryValue = new contentType();
						ElementToContents(e.Element, entry.summaryValue);
					}
					else
					{
						entry.contentValue = new contentType();
						ElementToContents(e.Element, entry.contentValue);
					}
				}

				if (e.ObjectBeingDeserialized is entryType_Blogger)
				{
					entryType_Blogger entry = (entryType_Blogger)e.ObjectBeingDeserialized;

					if (fSummary)
					{
						entry.summaryValue = new contentType();
						ElementToContents(e.Element, entry.summaryValue);
					}
					else
					{
						entry.contentValue = new contentType();
						ElementToContents(e.Element, entry.contentValue);
					}
				}
			}

			dbg.TraceOut();
		}
	} //end Atom

	#region "Fun" extensions
	/// <summary>
	/// BloggingFun: Various random methods not really atom-related, but fun.
	/// </summary>
	public class BloggingFun
	{
		private AtomDebug dbg = null;
		private TypePad.music m_music = null;
		private int m_napFound = 0;

		public BloggingFun()
		{
			dbg = new AtomDebug();
		}

		/// <summary>
		/// GetListeningTo: If the user is listening to music
		/// with either Windows Media Player (plugin required) or iTunes,
		/// this will return that information to you, for formatting in a post!
		/// </summary>
		public bool GetListeningTo(TypePad.music music)
		{
			dbg.TraceIn();

			bool ret = GetWMPoriTunes(music);

			if (!ret)
			{
				ret = GetNapster(music);
			}

			dbg.TraceOut();

			return ret;
		}

		private bool GetWMPoriTunes(TypePad.music music)
		{
			dbg.TraceIn();

			bool ret = false;

			try
			{
				music.artist = null;
				music.album = null;
				music.title = null;

				RegistryKey wmpKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\MediaPlayer\\CurrentMetadata");

				music.artist = (string)wmpKey.GetValue("Author");
				music.album = (string)wmpKey.GetValue("Album");
				music.title = (string)wmpKey.GetValue("Title");
						
				wmpKey.Close();

				if ((music.artist != null) && (music.album != null) && (music.title != null))
				{
					ret = true;
				}
			}
			catch
			{
			}

			dbg.TraceOut();

			return ret;
		}

		private bool GetNapster(TypePad.music music)
		{
			dbg.TraceIn();

			bool ret = false;

			IntPtr hwnd = FindWindow("PPAppFrameClass", "Napster");

			if (hwnd.ToInt32() != 0)
			{
				m_music = music;
				m_napFound = 0;

				EnumChildWindowsProc ecwp = new EnumChildWindowsProc(EnumNapsterChildWindows);
				EnumChildWindows(hwnd, ecwp, 0);
			}

			if ((music.artist != null) && (music.album != null) && (music.title != null))
			{
				ret = true;
			}

			dbg.TraceOut();

			return ret;
		}

		private string GetText(IntPtr hwnd)
		{
			dbg.TraceIn();

			string lpText = new string((char) 0, 100);
			int intLength = GetWindowText(hwnd, lpText, lpText.Length);
			if ((intLength > 0) && (intLength <= lpText.Length))
			{
				string lpText2 = new string((char)0, intLength);
				GetWindowText(hwnd, lpText2, lpText2.Length+1);
				dbg.TraceOut();
				return lpText2;
			}

			dbg.TraceOut();
			return null;
		}

		private bool EnumNapsterChildWindows(IntPtr hwnd, int lParam)
		{
			dbg.TraceIn();

			int windowID = GetWindowLong(hwnd, -12); //GWL_ID

			switch (windowID)
			{
				case 0x405: m_music.artist = GetText(hwnd); m_napFound++; break;
				case 0x406: m_music.album  = GetText(hwnd); m_napFound++; break;
				case 0x407: m_music.title  = GetText(hwnd); m_napFound++; break;
			}

			dbg.TraceOut();

			if (m_napFound == 3)
			{
				return false; //stop enum
			}

			return true;
		}

		private delegate bool EnumChildWindowsProc(IntPtr hwnd, int lParam);

		[DllImport("user32.dll")] 
		private static extern int EnumChildWindows(IntPtr hwndParent, EnumChildWindowsProc ecwp, int lParam); 

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		private static extern int GetWindowText(IntPtr hwnd, string lpString, int cch);

		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hwnd, int index);
	}
	#endregion

	#region TypePad extensions
	public class TypePad : Atom
	{
		public TypePad(Uri endPointURL, generatorType generator, string username, string password)
			: base(endPointURL, generator, username, password)
		{
			dbg.Out("Creating new TypePad object");
		}

		public class book
		{
			public string isbn;
			public string title;
			public string author;
			public int    rating;
			public string review;
		}

		public class person
		{
			public string homepageURL;
			public string name;
			public string weblogURL;
			public string emailAddress;
			public string foafURL;
			public string shortBio;
		}

		public class link
		{
			public string url;
			public string title;
			public string review;
		}

		public class music
		{
			public string title;
			public string album;
			public string artist;
			public string thumbnailURL;
			public int    rating;
			public string review;
		}

		public class photo
		{
			public string title;
			public string summary;
			public string filename;
			public string location;
			public DateTime taken;
			public Image image;
		}

		/// <summary>
		/// PostToBookList: Sends an ISBN to TypePad, which it will helpfully resolve against Amazon.com, and more
		/// </summary>
		public entryType PostToBookList(string postBookURL, book Book)
		{
			dbg.TraceIn();

			MemoryStream xmlstreamOut = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(xmlstreamOut, Encoding.ASCII);

			writer.WriteStartElement("entry");
			writer.WriteAttributeString("xmlns", "http://purl.org/atom/ns#");
			writer.WriteAttributeString("xmlns:book", "http://sixapart.com/atom/book#");
			writer.WriteAttributeString("xmlns:rvw", "http://purl.org/NET/RVW/0.1/");

			if ((Book.isbn != null) && (Book.isbn.Length > 0))
			{
				writer.WriteStartElement("book:isbn");
				writer.WriteString(Book.isbn);
				writer.WriteEndElement();
			}
		
			if ((Book.author != null) && (Book.author.Length > 0))
			{
				writer.WriteStartElement("book:author");
				writer.WriteString(Book.author);
				writer.WriteEndElement();
			}

			if (Book.rating != 0)
			{
				writer.WriteStartElement("rvw:value");
			
				//has to be 1 to 5
				int rating = Book.rating;
				if (rating > 5) rating = 5;
				if (rating < 1) rating = 1;

				writer.WriteString(Convert.ToString(rating, CultureInfo.InvariantCulture));
				writer.WriteEndElement();
			}

			if ((Book.review != null) && (Book.review.Length > 0))
			{
				writer.WriteStartElement("content");
				writer.WriteString(Book.review);
				writer.WriteEndElement();
			}

			if ((Book.title != null) && (Book.title.Length > 0))
			{
				writer.WriteStartElement("book:title");
				writer.WriteString(Book.title);
				writer.WriteEndElement();
			}

			writer.WriteElementString("issued", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));

			writer.WriteEndElement(); //entry
			writer.Flush();

			entryType entry = TypePadListPost(postBookURL, xmlstreamOut);

			writer.Close();

			dbg.TraceOut();

			return entry;
		}

		/// <summary>
		/// PostToPeopleList: People
		/// </summary>
		public entryType PostToPeopleList(string postPeopleURL, person Person)
		{
			dbg.TraceIn();

			MemoryStream xmlstreamOut = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(xmlstreamOut, Encoding.ASCII);

			writer.WriteStartElement("entry");
			writer.WriteAttributeString("xmlns", "http://purl.org/atom/ns#");
			writer.WriteAttributeString("xmlns:person", "http://sixapart.com/atom/person#");
			writer.WriteAttributeString("xmlns:bio", "http://purl.org/vocab/bio/0.1/");
			
			if ((Person.homepageURL != null) && (Person.homepageURL.Length > 0))
			{
				writer.WriteStartElement("person:homepage");
				writer.WriteString(Person.homepageURL);
				writer.WriteEndElement();
			}

			if ((Person.emailAddress != null) && (Person.emailAddress.Length > 0))
			{
				writer.WriteStartElement("person:email");
				writer.WriteString(Person.emailAddress);
				writer.WriteEndElement();
			}

			if ((Person.foafURL != null) && (Person.foafURL.Length > 0))
			{
				writer.WriteStartElement("person:foaf_url");
				writer.WriteString(Person.foafURL);
				writer.WriteEndElement();
			}

			if ((Person.name != null) && (Person.name.Length > 0))
			{
				writer.WriteStartElement("person:name");
				writer.WriteString(Person.name);
				writer.WriteEndElement();
			}

			if ((Person.weblogURL != null) && (Person.weblogURL.Length > 0))
			{
				writer.WriteStartElement("person:homepage_name");
				writer.WriteString(Person.weblogURL);
				writer.WriteEndElement();
			}

			if ((Person.shortBio != null) && (Person.shortBio.Length > 0))
			{
				writer.WriteStartElement("bio:olb");
				writer.WriteString(Person.shortBio);
				writer.WriteEndElement();
			}

			writer.WriteElementString("issued", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));

			writer.WriteEndElement(); //entry
			writer.Flush();

			entryType entry = TypePadListPost(postPeopleURL, xmlstreamOut);

			writer.Close();

			dbg.TraceOut();

			return entry;
		}

		/// <summary>
		/// PostToLinkList: Links
		/// </summary>
		public entryType PostToLinkList(string postLinkURL, link Link)
		{
			dbg.TraceIn();

			MemoryStream xmlstreamOut = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(xmlstreamOut, Encoding.ASCII);

			writer.WriteStartElement("entry");
			writer.WriteAttributeString("xmlns", "http://purl.org/atom/ns#");
			writer.WriteAttributeString("xmlns:link", "http://sixapart.com/atom/link#");

			if ((Link.review != null) && (Link.review.Length > 0))
			{
				writer.WriteStartElement("content");
				writer.WriteString(Link.review);
				writer.WriteEndElement();
			}

			if ((Link.url != null) && (Link.url.Length > 0))
			{
				writer.WriteStartElement("link:url");
				writer.WriteString(Link.url);
				writer.WriteEndElement();
			}

			if ((Link.title != null) && (Link.title.Length > 0))
			{
				writer.WriteStartElement("link:title");
				writer.WriteString(Link.title);
				writer.WriteEndElement();
			}

			writer.WriteElementString("issued", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));

			writer.WriteEndElement(); //entry
			writer.Flush();

			entryType entry = TypePadListPost(postLinkURL, xmlstreamOut);

			writer.Close();

			dbg.TraceOut();

			return entry;
		}

		/// <summary>
		/// PostToMusicList: Music
		/// </summary>
		public entryType PostToMusicList(string postMusicURL, music Music)
		{
			dbg.TraceIn();

			MemoryStream xmlstreamOut = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(xmlstreamOut, Encoding.ASCII);

			writer.WriteStartElement("entry");
			writer.WriteAttributeString("xmlns", "http://purl.org/atom/ns#");
			writer.WriteAttributeString("xmlns:music", "http://sixapart.com/atom/song#");
			writer.WriteAttributeString("xmlns:rvw", "http://purl.org/NET/RVW/0.1/");

			if ((Music.album != null) && (Music.album.Length > 0))
			{
				writer.WriteStartElement("music:album");
				writer.WriteString(Music.album);
				writer.WriteEndElement();
			}

			if ((Music.artist != null) && (Music.artist.Length > 0))
			{
				writer.WriteStartElement("music:artist");
				writer.WriteString(Music.artist);
				writer.WriteEndElement();
			}

			if ((Music.review != null) && (Music.review.Length > 0))
			{
				writer.WriteStartElement("content");
				writer.WriteString(Music.review);
				writer.WriteEndElement();
			}

			if ((Music.title != null) && (Music.title.Length > 0))
			{
				writer.WriteStartElement("music:title");
				writer.WriteString(Music.title);
				writer.WriteEndElement();
			}

			if ((Music.thumbnailURL != null) && (Music.thumbnailURL.Length > 0))
			{
				writer.WriteStartElement("music:thumbnail");
				writer.WriteString(Music.thumbnailURL);
				writer.WriteEndElement();
			}

			if (Music.rating != 0)
			{
				writer.WriteStartElement("rvw:value");
			
				//has to be 1 to 5
				int rating = Music.rating;
				if (rating > 5) rating = 5;
				if (rating < 1) rating = 1;

				writer.WriteString(Convert.ToString(rating, CultureInfo.InvariantCulture));
				writer.WriteEndElement();
			}


			writer.WriteElementString("issued", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));

			writer.WriteEndElement(); //entry
			writer.Flush();

			entryType entry = TypePadListPost(postMusicURL, xmlstreamOut);

			writer.Close();

			dbg.TraceOut();

			return entry;
		}

		/// <summary>
		/// PostToPhotos: Pictures!
		/// </summary>
		public entryType PostToPhotos(string postPhotoURL, photo Photo)
		{
			dbg.TraceIn();

			MemoryStream xmlstreamOut = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(xmlstreamOut, Encoding.ASCII);

			writer.WriteStartElement("entry");
			writer.WriteAttributeString("xmlns", "http://purl.org/atom/ns#");
			writer.WriteAttributeString("xmlns:photo", "http://sixapart.com/atom/photo#");
			
			if ((Photo.title != null) && (Photo.title.Length > 0))
			{
				writer.WriteStartElement("title");
				writer.WriteString(Photo.title);
				writer.WriteEndElement();
			}

			if ((Photo.filename != null) && (Photo.filename.Length > 0))
			{
				writer.WriteStartElement("photo:filename");
				writer.WriteString(Photo.filename);
				writer.WriteEndElement();
			}

			if ((Photo.location != null) && (Photo.location.Length > 0))
			{
				writer.WriteStartElement("photo:location");
				writer.WriteString(Photo.location);
				writer.WriteEndElement();
			}

			if ((Photo.summary != null) && (Photo.summary.Length > 0))
			{
				writer.WriteStartElement("summary");
				writer.WriteString(Photo.summary);
				writer.WriteEndElement();
			}

			if (Photo.taken.Ticks > 0)
			{
				writer.WriteStartElement("photo:taken");
				writer.WriteString(Photo.taken.ToString("s", CultureInfo.InvariantCulture));
				writer.WriteEndElement();
			}

			if (Photo.image != null)
			{
				MemoryStream contentStream = new MemoryStream();
				Photo.image.Save(contentStream, ImageFormat.Jpeg);
				string content64 = Convert.ToBase64String(contentStream.GetBuffer());

				writer.WriteStartElement("content");
				writer.WriteAttributeString("mode","base64");
				writer.WriteAttributeString("type","image/jpeg");
				writer.WriteString(content64);
				writer.WriteEndElement();
			}

			writer.WriteElementString("issued", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));

			writer.WriteEndElement(); //entry
			writer.Flush();

			entryType entry = TypePadListPost(postPhotoURL, xmlstreamOut);

			writer.Close();

			dbg.TraceOut();

			return entry;
		}
		
		/// <summary>
		/// UploadMimeFile: Handle service.upload URLs ... not sure how this is supposed to work yet ... no docs available
		/// </summary>
		public entryType UploadMimeFile(string uploadURL, Stream uploadStream, string mimeType, string fileName)
		{
			dbg.TraceIn();

			MemoryStream xmlstreamOut = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(xmlstreamOut, Encoding.ASCII);

			writer.WriteStartElement("entry");
			writer.WriteAttributeString("xmlns", "http://purl.org/atom/ns#");
			
			if ((fileName != null) && (fileName.Length > 0))
			{
				writer.WriteStartElement("title");
				writer.WriteString(fileName);
				writer.WriteEndElement();
			}

			if (uploadStream != null)
			{
				byte[] upBytes = new byte[uploadStream.Length];
				uploadStream.Read(upBytes, 0, (int)uploadStream.Length);
				string base64string = Convert.ToBase64String(upBytes, 0, (int)uploadStream.Length);

				writer.WriteStartElement("content");
				writer.WriteAttributeString("mode","base64");
				writer.WriteAttributeString("type",mimeType);
				writer.WriteString(base64string);
				writer.WriteEndElement();
			}

			writer.WriteElementString("issued", DateTime.Now.ToString("s", CultureInfo.InvariantCulture));

			writer.WriteEndElement(); //entry
			writer.Flush();

			entryType entry = TypePadListPost(uploadURL, xmlstreamOut);

			writer.Close();

			dbg.TraceOut();

			return entry;
		}
		
		/// <summary>
		/// GetCategories: Returns string array of TypePad categories
		/// </summary>
		public string[] GetCategories(string catURL)
		{
			dbg.TraceIn();

			HttpWebRequest webreq = (HttpWebRequest)HttpWebRequest.Create(catURL);

			if (m_proxy != null)
			{
				webreq.Proxy = m_proxy;
			}

			//build the get/post request by hand.
			webreq.UserAgent     = m_generator.Value;
			webreq.ContentType   = "application/xml";
			webreq.Method		 = "GET";

			//set up the password encryption
			GenerateXWSSEHeader(webreq, m_userName, m_password);

			HttpWebResponse webresp = (HttpWebResponse)webreq.GetResponse();

			if (webresp.StatusCode != HttpStatusCode.OK)
			{
				webresp.Close();
				throw(new WebException(webresp.StatusDescription));
			}

			XmlTextReader xmlReader = null;

			Stream xmlResp = webresp.GetResponseStream();
			xmlReader = new XmlTextReader(xmlResp);

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(xmlReader);

			XmlNodeList xmlNodes = xmlDoc.DocumentElement.ChildNodes;
			string[] catStr = new string[xmlNodes.Count];
			int x = 0;

			foreach (XmlNode node in xmlNodes)
			{
				catStr[x++] = node.InnerText;
			}

			dbg.TraceOut();

			return catStr;
		}

		/// <summary>
		/// TypePadListPost: Helper function to post to lists
		/// </summary>
		private entryType TypePadListPost(string postURL, MemoryStream xmlstreamOut)
		{
			dbg.TraceIn();

			HttpWebRequest webreq = (HttpWebRequest)HttpWebRequest.Create(postURL);

			if (m_proxy != null)
			{
				webreq.Proxy = m_proxy;
			}

			webreq.ContentLength = xmlstreamOut.Length;

			//build the get/post request by hand.
			webreq.UserAgent     = m_generator.Value;

			webreq.ContentType   = "application/xml";
			webreq.Method		 = "POST";

			//set up the password encryption
			GenerateXWSSEHeader(webreq, m_userName, m_password);

			Stream newStream = webreq.GetRequestStream();

			newStream.Write(xmlstreamOut.GetBuffer(), 0, (int)xmlstreamOut.Length);
			newStream.Flush();
			newStream.Close();

			HttpWebResponse webresp = (HttpWebResponse)webreq.GetResponse();

			if (webresp.StatusCode != HttpStatusCode.Created)
			{
				webresp.Close();
				throw(new WebException(webresp.StatusDescription));
			}

			Stream xmlResp = webresp.GetResponseStream();
			XmlTextReader xmlReader = new XmlTextReader(xmlResp);
		
			dbg.TraceOut();

			return DeserializeToEntry(xmlReader);
		}
	}
	#endregion

	#region Blogger extensions
	[System.Xml.Serialization.XmlRootAttribute("entry", Namespace="", IsNullable=false)]
	public class entryType_Blogger
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("issued", typeof(string))]
		public string issued;

		[System.Xml.Serialization.XmlElementAttribute("link", typeof(linkType))]
		public linkType[] links;

		[System.Xml.Serialization.XmlElementAttribute("contributor", typeof(authorType))]
		public authorType[] contributors;

		[System.Xml.Serialization.XmlElementAttribute("created", typeof(string))]
		public string created;

		[System.Xml.Serialization.XmlElementAttribute("id", typeof(string))]
		public string id;

		[System.Xml.Serialization.XmlElementAttribute("author", typeof(authorType))]
		public authorType author;

		[System.Xml.Serialization.XmlElementAttribute("title", typeof(string))]
		public string title;

		[System.Xml.Serialization.XmlElementAttribute("modified", typeof(string))]
		public string modified;

		//contentTypes are done by hand in a deserialization event
		public contentType summaryValue;
		public contentType contentValue;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified, Namespace="http://www.w3.org/XML/1998/namespace")]
		public string lang;
	}

	public class Blogger : Atom
	{
		public Blogger(Uri endPointURL, generatorType generator, string username, string password)
			: base(endPointURL, generator, username, password)
		{
			dbg.Out("Creating new Blogger object");
		}

		/// <summary>
		/// ConvertToBloggerEntry: Changes an entry with no namespace into one with one
		/// </summary>
		public entryType ConvertToBloggerEntry(XmlTextReader xmlText)
		{
			dbg.TraceIn();

			XmlSerializer serializer = new XmlSerializer(typeof(entryType_Blogger));
			serializer.UnknownElement += new XmlElementEventHandler(Serializer_contentType);

			entryType_Blogger blogEntry = (entryType_Blogger)serializer.Deserialize(xmlText);

			entryType entryNamespace = new entryType();

			entryNamespace.author = blogEntry.author;
			entryNamespace.created = blogEntry.created;
			entryNamespace.id = blogEntry.id;
			entryNamespace.issued = blogEntry.issued;
			entryNamespace.links = blogEntry.links;
			entryNamespace.modified = blogEntry.modified;
			entryNamespace.contentValue = blogEntry.contentValue;
			entryNamespace.contributors = blogEntry.contributors;
			entryNamespace.lang = blogEntry.lang;
			entryNamespace.summaryValue = blogEntry.summaryValue;
			entryNamespace.title = blogEntry.title;

			dbg.TraceOut();

			return entryNamespace;
		}

		public override service[] GetServices()
		{
			dbg.TraceIn();

			service[] services = base.GetServices();

			foreach (service serv in services)
			{
				serv.homepageURL = GetHomepage(serv.postURL);
			}

			dbg.TraceOut();

			return services;
		}

		
		/// <summary>
		/// Blogger::GetHomepage: Actually quite blogger-specific, so let this fail.
		/// </summary>
		/// 
		public string GetHomepage(string postURL)
		{
			dbg.TraceIn();

			string homePage = "";

			try
			{
				//the ID of the blog in blogger is the last part of the atomPostUrl
				int idPartStart = postURL.LastIndexOf('/');
				if (idPartStart > 0)
				{
					string strBlogId = postURL.Substring(idPartStart+1);
					string blogURL = "http://www.blogger.com/rsd.pyra?blogID="+strBlogId;

					HttpWebRequest webreq = (HttpWebRequest)HttpWebRequest.Create(blogURL);

					if (m_proxy != null)
					{
						webreq.Proxy = m_proxy;
					}

					webreq.UserAgent     = m_generator.Value;
					webreq.Method        = "GET";
					HttpWebResponse webresp = (HttpWebResponse)webreq.GetResponse();
					if (webresp.StatusCode != HttpStatusCode.OK)
					{
						webresp.Close();
						throw(new WebException(webresp.StatusDescription));
					}

					Stream xmlResp = webresp.GetResponseStream();
					
					//ugh, there is whitespace at the top of the file,
					//which the XML reader will choke on if we don't skip it.
					byte[] xmlBytes = new byte[webresp.ContentLength];
					xmlResp.Read(xmlBytes, 0, (int)webresp.ContentLength);
					xmlResp.Close();

					int index = 0;
					char foundChar = ' ';
					while (foundChar != '<')
					{
						foundChar = (char)xmlBytes[index++];
					}

					//since the XML stream that we have doesn't support seeking,
					//we can't back up. We have to create a new stream with the
					//corrected data.
					Stream xmlRespCorrected = new MemoryStream(xmlBytes, index-1, (int)webresp.ContentLength-index-1, false, false);

					XmlTextReader xmlReader = new XmlTextReader(xmlRespCorrected);
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.Load(xmlReader);

					//couldn't get xpath query to work for some reason,
					//go ahead and just walk the DOM and find what we want
					XmlNodeList xmlNodes = xmlDoc.DocumentElement.ChildNodes;
					foreach (XmlNode xmlNode in xmlNodes)
					{
						if (xmlNode.Name.Equals("service"))
						{
							XmlNodeList xmlNodesService = xmlNode.ChildNodes;
							foreach (XmlNode xmlNodeService in xmlNodesService)
							{
								if (xmlNodeService.Name.Equals("homePageLink"))
								{
									dbg.TraceOut();
									return xmlNodeService.InnerText;
								}
							}
						}
					}
				}
			}
			catch{} //oh well, no big deal if this doesn't work, just not as pretty

			dbg.TraceOut();

			return homePage;
		}
	}
	#endregion

	#region Various Helper Classes
	public enum serviceType
	{
		post,
		feed,
		firstFeed,
		prevFeed,
		nextFeed,
		upload,
		categories,
		edit,
		alternate,
		unknown
	}
	
	/// <summary>
	/// service: A class that holds information about the user's blogs and other service endpoints.
	/// </summary>
	public class service
	{
		public serviceType srvType;
		public string name;
		public string postURL;
		public string homepageURL;

		public override string ToString()
		{
			return name;
		}
	}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://purl.org/atom/ns#")]
	[System.Xml.Serialization.XmlRootAttribute("feed", Namespace="http://purl.org/atom/ns#", IsNullable=false)]
	public class feedType 
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("link", typeof(linkType))]
		public linkType[] links;

		[System.Xml.Serialization.XmlElementAttribute("contributor", typeof(authorType))]
		public authorType[] contributors;

		[System.Xml.Serialization.XmlElementAttribute("author", typeof(authorType))]
		public authorType author;

		[System.Xml.Serialization.XmlElementAttribute("id", typeof(string), DataType="anyURI")]
		public string id;

		[System.Xml.Serialization.XmlElementAttribute("generator", typeof(generatorType))]
		public generatorType generator;

		[System.Xml.Serialization.XmlElementAttribute("title", typeof(string))]
		public string title;

		[System.Xml.Serialization.XmlElementAttribute("tagline", typeof(string))]
		public string tagline;

		[System.Xml.Serialization.XmlElementAttribute("modified", typeof(string))]
		public string modified;

		[System.Xml.Serialization.XmlElementAttribute("entry", typeof(entryType))]
		public entryType[] entries;
		
		[System.Xml.Serialization.XmlElementAttribute("copyright", typeof(string))]
		public string copyright;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(DataType="normalizedString")]
		public string version;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified, Namespace="http://www.w3.org/XML/1998/namespace")]
		public string lang;
	}

	/// <remarks/>
	public class authorType 
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("url", typeof(string), DataType="anyURI")]
		public string url;

		[System.Xml.Serialization.XmlElementAttribute("email", typeof(string), DataType="normalizedString")]
		public string email;

		[System.Xml.Serialization.XmlElementAttribute("name", typeof(string))]
		public string name;
	}

	/// <remarks/>
	public class linkType 
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string rel;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(DataType="normalizedString")]
		public string type;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(DataType="anyURI")]
		public string href;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string title;

		public serviceType srvType;
	}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://purl.org/atom/ns#")]
	[System.Xml.Serialization.XmlRootAttribute("entry", Namespace="http://purl.org/atom/ns#", IsNullable=false)]
	public class entryType 
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("title", typeof(string))]
		public string title;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("link", typeof(linkType))]
		public linkType[] links;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("author", typeof(authorType))]
		public authorType author;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("contributor", typeof(authorType))]
		public authorType[] contributors;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("id", typeof(string))]
		public string id;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("issued", typeof(string))]
		public string issued;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("modified", typeof(string))]
		public string modified;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("created", typeof(string))]
		public string created;

		/*
				* Serializer isn't very good at handling this.  We'll do it "by hand" with an event
				*
				*/
		public contentType summaryValue;

		/*
				* Serializer isn't very good at handling this.  We'll do it "by hand" with an event
				*
				*/
		public contentType contentValue;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Qualified, Namespace="http://www.w3.org/XML/1998/namespace")]
		public string lang;
	}

	/// <remarks/>
	public class contentType 
	{
		public string Text;
		public string type;
		public string mode;
		public string lang;
	}

	/// <remarks/>
	public class generatorType 
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(DataType="anyURI")]
		public string url;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string version;

		/// <remarks/>
		[System.Xml.Serialization.XmlTextAttribute()]
		public string Value;
	}
	#endregion

	#region Debugging
	public class AtomDebug
	{
		public void Out(string str)
		{
			#if DEBUG
			Debug.WriteLine("ATOM: " + str);
			#endif
		}

		public void TraceIn()
		{
			#if DEBUG
			try
			{
				StackTrace st = new StackTrace(new StackFrame(1, true));
				MethodBase method = st.GetFrame(0).GetMethod();
				Out("Entering '"+ method.DeclaringType.Name + ":" + method.Name + "'");
				Debug.Indent();
			}
			catch{} //don't fail if we're just trying to get debug info!
			#endif
		}

		public void TraceOut()
		{
			#if DEBUG
			try
			{
				Debug.Unindent();
				StackTrace st = new StackTrace(new StackFrame(1, true));
				MethodBase method = st.GetFrame(0).GetMethod();
				Out("Exiting '"+ method.DeclaringType.Name + ":" + method.Name + "'");
			}
			catch{} //don't fail if we're just trying to get debug info!
			#endif
		}
	}
	#endregion
}
