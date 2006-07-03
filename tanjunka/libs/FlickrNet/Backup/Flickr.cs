using System;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

namespace FlickrNet
{
	/// <summary>
	/// The main Flickr class.
	/// </summary>
	/// <remarks>
	/// Create an instance of this class and then call its methods to perform methods on Flickr.
	/// </remarks>
	/// <example>
	/// <code>FlickrNet.Flickr flickr = new FlickrNet.Flickr();
	/// User user = flickr.PeopleFindByEmail("cal@iamcal.com");
	/// Console.WriteLine("User Id is " + u.UserId);</code>
	/// </example>
	public class Flickr
	{
		#region [ Private Variables ]
		private const string _baseUrl = "http://www.flickr.com/services/rest/";
		private const string _uploadUrl = "http://www.flickr.com/tools/uploader_go.gne";
		private string _apiKey = "";
		private string _email = "";
		private string _password = "";
		private int _timeout = 30000;
		private const string UserAgent = "Mozilla/4.0 FlickrNet API (compatible; MSIE 6.0; Windows NT 5.1)";

		private WebProxy _proxy = WebProxy.GetDefaultProxy();

		internal string ApiKey { get { return _apiKey; } }
		#endregion

		public TimeSpan CacheTimeout
		{
			get { return Cache.CacheTimeout; }
			set { Cache.CacheTimeout = value; }
		}

		public void ForceCacheSave()
		{
			Cache.ForceSave();
		}

		public int HttpTimeout
		{
			get { return _timeout; } 
			set { _timeout = value; }
		}

		/// <summary>
		/// Get or set the authentication email. Only needed for certain methods.
		/// </summary>
		public string Email { get { return _email; } set { _email = value; } }

		/// <summary>
		/// Set the password used for authentication. Only needed for certain methods.
		/// </summary>
		/// <param name="password">The password to be used.</param>
		public void SetPassword(string password) { _password = password; }

		/// <summary>
		/// You can set the <see cref="WebProxy"/> or alter its properties.
		/// It defaults to your internet explorer proxy settings.
		/// </summary>
		public WebProxy Proxy { get { return _proxy; } set { _proxy = value; } }

		/// <summary>
		/// Create a new instance of the <see cref="Flickr"/> class with no email or password.
		/// </summary>
		public Flickr(string apiKey) : this(apiKey, "", "")
		{
		}

		/// <summary>
		/// Create a new instance of the <see cref="Flickr"/> class with the email address and password given
		/// </summary>
		/// <param name="apiKey">The API Key for to use.</param>
		/// <param name="email">The email address used for authentication.</param>
		/// <param name="password">The password used for authentication.</param>
		public Flickr(string apiKey, string email, string password)
		{
			_apiKey = apiKey;
			_email = email;
			_password = password;
		}

		private FlickrNet.Response DoGetResponse(string url)
		{
			HttpWebRequest req = null;
			HttpWebResponse res = null;

			try
			{
				req = (HttpWebRequest)HttpWebRequest.Create(url);
				req.UserAgent = UserAgent;
				req.Proxy = Proxy;
				req.Timeout = HttpTimeout;
				res = (HttpWebResponse)req.GetResponse();
			}
			catch(WebException ex)
			{
				if( ex.Status == WebExceptionStatus.ProtocolError )
				{
					HttpWebResponse res2 = (HttpWebResponse)ex.Response;
					if( res2 != null )
					{
						throw new FlickrException((int)res2.StatusCode, res2.StatusDescription);
					}
				}
				throw new FlickrException(9999, ex.Message);
			}

			XmlSerializer serializer = new XmlSerializer(typeof(FlickrNet.Response));

			string responseString = string.Empty;

			using (StreamReader sr = new StreamReader(res.GetResponseStream()))
			{
				responseString = sr.ReadToEnd();
			}

			try
			{
				StringReader responseReader = new StringReader(responseString);
				FlickrNet.Response response = (FlickrNet.Response)serializer.Deserialize(responseReader);
				responseReader.Close();

				return response;
			}
			catch(InvalidOperationException ex)
			{
				// Serialization error occurred!
				throw new FlickrException(9998, "Invalid response received (" + ex.Message + ")");
			}
		}

		protected Stream DoDownloadPicture(string url)
		{
			HttpWebRequest req = null;
			HttpWebResponse res = null;

			try
			{
				req = (HttpWebRequest)HttpWebRequest.Create(url);
				req.UserAgent = UserAgent;
				req.Proxy = Proxy;
				req.Timeout = HttpTimeout;
				res = (HttpWebResponse)req.GetResponse();
			}
			catch(WebException ex)
			{
				if( ex.Status == WebExceptionStatus.ProtocolError )
				{
					HttpWebResponse res2 = (HttpWebResponse)ex.Response;
					if( res2 != null )
					{
						throw new FlickrException((int)res2.StatusCode, res2.StatusDescription);
					}
				}
				else if( ex.Status == WebExceptionStatus.Timeout )
				{
					throw new FlickrException(301, "Request time-out");
				}
				throw new FlickrException(9999, "Picture download failed (" + ex.Message + ")");
			}

			return res.GetResponseStream();
		}

		protected Response GetResponseNoCache(string url)
		{
			return GetResponse(url, TimeSpan.MinValue);
		}

		protected Response GetResponseAlwaysCache(string url)
		{
			return GetResponse(url, TimeSpan.MaxValue);
		}

		protected Response GetResponseCache(string url)
		{
			return GetResponse(url, Cache.CacheTimeout);
		}

		protected Response GetResponse(string url, TimeSpan cacheTimeout)
		{
			System.Diagnostics.Debug.WriteLine("URL Request: " + url);
			if( cacheTimeout == TimeSpan.MinValue )
			{
				System.Diagnostics.Debug.WriteLine("Not cached");
				return DoGetResponse(url);
			}

			if( cacheTimeout == TimeSpan.MaxValue && Cache.ResponseCache.ContainsKey(url) )
			{
				System.Diagnostics.Debug.WriteLine("Found in cache: Always cache");
				Cache.ResponseCacheItem cacheItem = (Cache.ResponseCacheItem)Cache.ResponseCache[url];
				return cacheItem.response;
			}

			if( Cache.ResponseCache.ContainsKey(url) )
			{
				System.Diagnostics.Debug.Write("Found in cache: ");
				Cache.ResponseCacheItem cacheItem = (Cache.ResponseCacheItem)Cache.ResponseCache[url];
				if( cacheItem.creationTime.Add(cacheTimeout) < DateTime.Now )
				{
					System.Diagnostics.Debug.WriteLine("Timed out");
					Cache.ResponseCache.Remove(url);
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("Returned");
					return cacheItem.response;
				}
			}

			System.Diagnostics.Debug.WriteLine("Retrieve anyway");
			Cache.ResponseCacheItem resCache = new Cache.ResponseCacheItem();
			resCache.response = DoGetResponse(url);
			resCache.url = url;
			resCache.creationTime = DateTime.Now;

			if( resCache.response.Status == ResponseStatus.OK )
			{
				Cache.ResponseCache.Add(url, resCache);
			}

			return resCache.response;
		}

		/// <summary>
		/// Downloads the picture from a internet and transfers it to a stream object.
		/// </summary>
		/// <param name="url">The url of the image to download.</param>
		/// <returns>A <see cref="Stream"/> object containing the downloaded picture.</returns>
		public System.IO.Stream DownloadPicture(string url)
		{
			System.Diagnostics.Debug.WriteLine("Photo url: " + url);

			if( Cache.DownloadCache.ContainsKey(url) )
			{
				System.Diagnostics.Debug.WriteLine("Cache found");
				Cache.PictureCacheItem cacheItem = (Cache.PictureCacheItem)Cache.DownloadCache[url];
				return Utils.GetApplicationDataReadStream(cacheItem.filename);
			}

			System.Diagnostics.Debug.WriteLine("No cache found");
			Cache.PictureCacheItem picCache = new Cache.PictureCacheItem();
			picCache.filename = Guid.NewGuid().ToString();
			Stream read = DoDownloadPicture(url);
			Stream write = Utils.GetApplicationDataWriteStream(picCache.filename);
			int b = read.ReadByte();
			long fileSize = 0;
			while( b != -1 )
			{
				fileSize++;
				write.WriteByte((byte)b);
				b = read.ReadByte();
			}
			read.Close();
			write.Close();

			while( Cache.CacheSize + fileSize > Cache.CacheSizeLimit )
			{
				System.Diagnostics.Debug.WriteLine("Cache full");
				Cache.DeleteOldest();
			}

			Cache.CacheSize += fileSize;

			picCache.url = url;
			picCache.creationTime = DateTime.Now;
			picCache.fileSize = fileSize;

			Cache.DownloadCache.Add(url, picCache);
			Cache.ForceSave();

			return Utils.GetApplicationDataReadStream(picCache.filename);
		}

		#region [ UploadPicture ]
		/// <summary>
		/// Uploads a file to Flickr.
		/// </summary>
		/// <param name="filename">The filename of the file to open.</param>
		/// <returns>The id of the photo on a successful upload.</returns>
		/// <exception cref="FlickrException">Thrown when Flickr returns an error. see http://www.flickr.com/services/api/upload.api.html for more details.</exception>
		/// <remarks>Other exceptions may be thrown, see <see cref="FileStream"/> constructors for more details.</remarks>
		public string UploadPicture(string filename)
		{
			return UploadPicture(filename, null, null, null);
		}

		/// <summary>
		/// Uploads a file to Flickr.
		/// </summary>
		/// <param name="filename">The filename of the file to open.</param>
		/// <param name="title">The title of the photograph.</param>
		/// <returns>The id of the photo on a successful upload.</returns>
		/// <exception cref="FlickrException">Thrown when Flickr returns an error. see http://www.flickr.com/services/api/upload.api.html for more details.</exception>
		/// <remarks>Other exceptions may be thrown, see <see cref="FileStream"/> constructors for more details.</remarks>
		public string UploadPicture(string filename, string title)
		{
			return UploadPicture(filename, null, null, null);
		}

		/// <summary>
		/// Uploads a file to Flickr.
		/// </summary>
		/// <param name="filename">The filename of the file to open.</param>
		/// <param name="title">The title of the photograph.</param>
		/// <param name="description">The description of the photograph.</param>
		/// <returns>The id of the photo on a successful upload.</returns>
		/// <exception cref="FlickrException">Thrown when Flickr returns an error. see http://www.flickr.com/services/api/upload.api.html for more details.</exception>
		/// <remarks>Other exceptions may be thrown, see <see cref="FileStream"/> constructors for more details.</remarks>
		public string UploadPicture(string filename, string title, string description)
		{
			return UploadPicture(filename, null, null, null);
		}

		/// <summary>
		/// Uploads a file to Flickr.
		/// </summary>
		/// <param name="filename">The filename of the file to open.</param>
		/// <param name="title">The title of the photograph.</param>
		/// <param name="description">The description of the photograph.</param>
		/// <param name="tags">A comma seperated list of the tags to assign to the photograph.</param>
		/// <returns>The id of the photo on a successful upload.</returns>
		/// <exception cref="FlickrException">Thrown when Flickr returns an error. see http://www.flickr.com/services/api/upload.api.html for more details.</exception>
		/// <remarks>Other exceptions may be thrown, see <see cref="FileStream"/> constructors for more details.</remarks>
		public string UploadPicture(string filename, string title, string description, string tags)
		{
			Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			return UploadPicture(filename, stream, title, description, tags, -1, -1, -1);
		}

		/// <summary>
		/// Uploads a file to Flickr.
		/// </summary>
		/// <param name="filename">The filename of the file to open.</param>
		/// <param name="title">The title of the photograph.</param>
		/// <param name="description">The description of the photograph.</param>
		/// <param name="tags">A comma seperated list of the tags to assign to the photograph.</param>
		/// <param name="isPublic">True if the photograph should be public and false if it should be private.</param>
		/// <param name="isFriend">True if the photograph should be marked as viewable by friends contacts.</param>
		/// <param name="isFamily">True if the photograph should be marked as viewable by family contacts.</param>
		/// <returns>The id of the photo on a successful upload.</returns>
		/// <exception cref="FlickrException">Thrown when Flickr returns an error. see http://www.flickr.com/services/api/upload.api.html for more details.</exception>
		/// <remarks>Other exceptions may be thrown, see <see cref="FileStream"/> constructors for more details.</remarks>
		public string UploadPicture(string filename, string title, string description, string tags, bool isPublic, bool isFamily, bool isFriend)
		{
			Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			return UploadPicture(filename, stream, title, description, tags, isPublic?1:0, isFamily?1:0, isFriend?1:0);
		}

		/// <summary>
		/// Private method that does all the uploading work.
		/// </summary>
		/// <param name="filename">The filename of the file to be uploaded.</param>
		/// <param name="stream">The <see cref="Stream"/> object containing the pphoto to be uploaded.</param>
		/// <param name="title">The title of the photo (optional).</param>
		/// <param name="description">The description of the photograph (optional).</param>
		/// <param name="tags">The tags for the photograph (optional).</param>
		/// <param name="isPublic">0 for private, 1 for public.</param>
		/// <param name="isFamily">1 if family, 0 is not.</param>
		/// <param name="isFriend">1 if friend, 0 if not.</param>
		/// <returns>The id of the photograph after successful uploading.</returns>
		private string UploadPicture(string filename, Stream stream, string title, string description, string tags, int isPublic, int isFamily, int isFriend)
		{
			/*
			 * 
			 * Modified UploadPicture code taken from the Flickr.Net library
			 * URL: http://workspaces.gotdotnet.com/flickrdotnet
			 * It is used under the terms of the Common Public License 1.0
			 * URL: http://www.opensource.org/licenses/cpl.php
			 * 
			 * */

			string boundary = "FLICKR_MIME_" + DateTime.Now.ToString("yyyyMMddhhmmss");

			HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(_uploadUrl);
			req.UserAgent = "Mozilla/4.0 FlickrNet API (compatible; MSIE 6.0; Windows NT 5.1)";
			req.Method = "POST";
			req.Proxy = Proxy;
			req.Referer = "http://www.flickr.com";
			req.ContentType = "multipart/form-data; boundary=\"" + boundary + "\"";

			StringBuilder sb = new StringBuilder();
			// Email
			sb.Append("--" + boundary + "\r\n");
			sb.Append("Content-Disposition: form-data; name=\"email\"\r\n");
			sb.Append("\r\n");
			sb.Append(_email + "\r\n");

			// Password
			sb.Append("--" + boundary + "\r\n");
			sb.Append("Content-Disposition: form-data; name=\"password\"\r\n");
			sb.Append("\r\n");
			sb.Append(_password + "\r\n");

			if( title != null && title.Length > 0 )
			{
				// Title
				sb.Append("--" + boundary + "\r\n");
				sb.Append("Content-Disposition: form-data; name=\"title\"\r\n");
				sb.Append("\r\n");
				sb.Append(title + "\r\n");
			}

			if( title != null && title.Length > 0 )
			{
				// Description
				sb.Append("--" + boundary + "\r\n");
				sb.Append("Content-Disposition: form-data; name=\"description\"\r\n");
				sb.Append("\r\n");
				sb.Append(description + "\r\n");
			}

			if( title != null && title.Length > 0 )
			{
				// Tags
				sb.Append("--" + boundary + "\r\n");
				sb.Append("Content-Disposition: form-data; name=\"tags\"\r\n");
				sb.Append("\r\n");
				sb.Append(tags + "\r\n");
			}

			if( isPublic >= 0 )
			{
				// IsPublic
				sb.Append("--" + boundary + "\r\n");
				sb.Append("Content-Disposition: form-data; name=\"is_public\"\r\n");
				sb.Append("\r\n");
				sb.Append(isPublic + "\r\n");
			}

			if( isPublic >= 0 )
			{
				// IsFriend
				sb.Append("--" + boundary + "\r\n");
				sb.Append("Content-Disposition: form-data; name=\"is_friend\"\r\n");
				sb.Append("\r\n");
				sb.Append(isFriend + "\r\n");
			}

			if( isPublic >= 0 )
			{
				// IsFamily
				sb.Append("--" + boundary + "\r\n");
				sb.Append("Content-Disposition: form-data; name=\"is_family\"\r\n");
				sb.Append("\r\n");
				sb.Append(isFamily + "\r\n");
			}

			// Photo
			sb.Append("--" + boundary + "\r\n");
			sb.Append("Content-Disposition: form-data; name=\"photo\"; filename=\"" + filename + "\"\r\n");
			sb.Append("Content-Type: image/jpeg\r\n");
			sb.Append("\r\n");

			UTF8Encoding encoding = new UTF8Encoding();

			byte[] postContents = encoding.GetBytes(sb.ToString());
			
			byte[] photoContents = new byte[stream.Length];
			stream.Read(photoContents, 0, photoContents.Length);
			stream.Close();

			byte[] postFooter = encoding.GetBytes("\r\n--" + boundary + "--\r\n");

			byte[] dataBuffer = new byte[postContents.Length + photoContents.Length + postFooter.Length];
			Buffer.BlockCopy(postContents, 0, dataBuffer, 0, postContents.Length);
			Buffer.BlockCopy(photoContents, 0, dataBuffer, postContents.Length, photoContents.Length);
			Buffer.BlockCopy(postFooter, 0, dataBuffer, postContents.Length + photoContents.Length, postFooter.Length);

			req.ContentLength = dataBuffer.Length;

			//DEBUG CODE
			Stream fs = File.Create("F:\\Temp\\output.txt");
			fs.Write(dataBuffer, 0, dataBuffer.Length);
			fs.Close();

			Stream resStream = req.GetRequestStream();
			resStream.Write(dataBuffer, 0, dataBuffer.Length);
			resStream.Close();

			HttpWebResponse res = (HttpWebResponse)req.GetResponse();

			XmlSerializer serializer = new XmlSerializer(typeof(FlickrNet.Uploader));

			StreamReader sr = new StreamReader(res.GetResponseStream());
			string s= sr.ReadToEnd();
			sr.Close();

			StringReader str = new StringReader(s);

			FlickrNet.Uploader uploader = (FlickrNet.Uploader)serializer.Deserialize(str);
			
			if( uploader.Status == ResponseStatus.OK )
			{
				return uploader.PhotoId;
			}
			else
			{
				throw new FlickrException(uploader.Code, uploader.Message);
			}
		}
		#endregion

		#region [ Blogs ]
		/// <summary>
		/// Gets a list of blogs that have been set up by the user.
		/// Requires authentication.
		/// </summary>
		/// <returns>A <see cref="Blogs"/> object containing the list of blogs.</returns>
		/// <remarks></remarks>
		public Blogs BlogGetList()
		{
			string url = _baseUrl + "?method=flickr.blogs.getList&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Blogs;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Posts a photo already uploaded to a blog.
		/// Requires authentication.
		/// </summary>
		/// <param name="blogId">The Id of the blog to post the photo too.</param>
		/// <param name="photoId">The Id of the photograph to post.</param>
		/// <param name="title">The title of the blog post.</param>
		/// <param name="description">The body of the blog post.</param>
		/// <returns>True if the operation is successful.</returns>
		public bool BlogPostPhoto(int blogId, int photoId, string title, string description)
		{
			return BlogPostPhoto(blogId, photoId, title, description, null);
		}

		/// <summary>
		/// Posts a photo already uploaded to a blog.
		/// Requires authentication.
		/// </summary>
		/// <param name="blogId">The Id of the blog to post the photo too.</param>
		/// <param name="photoId">The Id of the photograph to post.</param>
		/// <param name="title">The title of the blog post.</param>
		/// <param name="description">The body of the blog post.</param>
		/// <param name="blogPassword">The password of the blog if it is not already stored in flickr.</param>
		/// <returns>True if the operation is successful.</returns>
		public bool BlogPostPhoto(int blogId, int photoId, string title, string description, string blogPassword)
		{
			string url = _baseUrl + "?method=flickr.blogs.postPhoto&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "title=" + Utils.UrlEncode(title);
			url += "description=" + Utils.UrlEncode(description);
			if( blogPassword != null ) url += "blog_password=" + blogPassword;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}
		#endregion

		#region [ Contacts ]
		/// <summary>
		/// Gets a list of contacts for the logged in user.
		/// Requires authentication.
		/// </summary>
		/// <returns>An instance of the <see cref="Contacts"/> class containing the list of contacts.</returns>
		public Contacts ContactsGetList()
		{
			string url = _baseUrl + "?method=flickr.contacts.getList&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Contacts;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Gets a list of the given users contact, or those that are publically avaiable.
		/// </summary>
		/// <param name="userId">The Id of the user who's contacts you want to return.</param>
		/// <returns>An instance of the <see cref="Contacts"/> class containing the list of contacts.</returns>
		public Contacts ContactsGetPublicList(string userId)
		{
			string url = _baseUrl + "?method=flickr.contacts.getPublicList&api_key=" + _apiKey + "&user_id=" + userId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Contacts;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}
		#endregion

		#region [ Favorites ]
		/// <summary>
		/// Adds a photo to the logged in favourites.
		/// Requires authentication.
		/// </summary>
		/// <param name="photoId">The id of the photograph to add.</param>
		/// <returns>True if the operation is successful.</returns>
		public bool FavoritesAdd(string photoId)
		{
			string url = _baseUrl + "?method=flickr.favorites.add&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password + "&photo_id=" + photoId;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Removes a photograph from the logged in users favourites.
		/// Requires authentication.
		/// </summary>
		/// <param name="photoId">The id of the photograph to remove.</param>
		/// <returns>True if the operation is successful.</returns>
		public bool FavoritesRemove(string photoId)
		{
			string url = _baseUrl + "?method=flickr.favorites.remove&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password + "&photo_id=" + photoId;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Get a list of the currently logger in users favourites.
		/// Requires authentication.
		/// </summary>
		/// <returns><see cref="Photos"/> instance containing a collection of <see cref="Photo"/> objects.</returns>
		public Photos FavoritesGetList()
		{
			return FavoritesGetList(null, 0, 0);
		}

		/// <summary>
		/// Get a list of the currently logger in users favourites.
		/// Requires authentication.
		/// </summary>
		/// <param name="perPage">Number of photos to include per page.</param>
		/// <param name="page">The page to download this time.</param>
		/// <returns><see cref="Photos"/> instance containing a collection of <see cref="Photo"/> objects.</returns>
		public Photos FavoritesGetList(int perPage, int page)
		{
			return FavoritesGetList(null, perPage, page);
		}

		/// <summary>
		/// Get a list of favourites for the specified user.
		/// </summary>
		/// <param name="userId">The user id of the user whose favourites you wish to retrieve.</param>
		/// <returns><see cref="Photos"/> instance containing a collection of <see cref="Photo"/> objects.</returns>
		public Photos FavoritesGetList(string userId)
		{
			return FavoritesGetList(userId, 0, 0);
		}

		/// <summary>
		/// Get a list of favourites for the specified user.
		/// </summary>
		/// <param name="userId">The user id of the user whose favourites you wish to retrieve.</param>
		/// <param name="perPage">Number of photos to include per page.</param>
		/// <param name="page">The page to download this time.</param>
		/// <returns><see cref="Photos"/> instance containing a collection of <see cref="Photo"/> objects.</returns>
		public Photos FavoritesGetList(string userId, int perPage, int page)
		{
			string url = _baseUrl + "?method=flickr.favorites.getList&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			if( userId != null ) url += "&user_id=" + userId;
			if( perPage > 0 ) url += "&per_page=" + perPage;
			if( page > 0 ) url += "&page=" + page;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photos;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Gets the public favourites for a specified user.
		/// </summary>
		/// <remarks>This function difers from <see cref="Flickr.FavoritesGetList"/> in that the user id 
		/// is not optional.</remarks>
		/// <param name="userId">The is of the user whose favourites you wish to return.</param>
		/// <returns>A <see cref="Photos"/> object containing a collection of <see cref="Photo"/> objects.</returns>
		public Photos FavoritesGetPublicList(string userId)
		{
			return FavoritesGetPublicList(userId, 0, 0);
		}
			
		/// <summary>
		/// Gets the public favourites for a specified user.
		/// </summary>
		/// <remarks>This function difers from <see cref="Flickr.FavoritesGetList"/> in that the user id 
		/// is not optional.</remarks>
		/// <param name="userId">The is of the user whose favourites you wish to return.</param>
		/// <param name="perPage">The number of photos to return per page.</param>
		/// <param name="page">The specific page to return.</param>
		/// <returns>A <see cref="Photos"/> object containing a collection of <see cref="Photo"/> objects.</returns>
		public Photos FavoritesGetPublicList(string userId, int perPage, int page)
		{
			string url = _baseUrl + "?method=flickr.favorites.getPublicList&api_key=" + _apiKey + "&user_id=" + userId;
			if( perPage > 0 ) url += "&per_page=" + perPage;
			if( page > 0 ) url += "&page=" + page;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photos;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}
		#endregion

		#region [ Groups ]
		/// <summary>
		/// Returns the top <see cref="Category"/> with a list of sub-categories and groups. 
		/// (The top category does not have any groups in it but others may).
		/// </summary>
		/// <returns>A <see cref="Category"/> instance.</returns>
		public Category GroupsBrowse()
		{
			return GroupsBrowse(0);
		}
		
		/// <summary>
		/// Returns the <see cref="Category"/> specified by the category id with a list of sub-categories and groups. 
		/// </summary>
		/// <param name="catId"></param>
		/// <returns>A <see cref="Category"/> instance.</returns>
		public Category GroupsBrowse(long catId)
		{
			string url = _baseUrl + "?method=flickr.groups.browse&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			if( catId != 0 ) url += "&cat_id=" + catId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Category;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Returns a list of currently active groups.
		/// </summary>
		/// <returns>An <see cref="ActiveGroups"/> instance.</returns>
		public ActiveGroups GroupsGetActiveList()
		{
			string url = _baseUrl + "?method=flickr.groups.getActiveList&api_key=" + _apiKey;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.ActiveGroups;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Returns a <see cref="GroupInfo"/> object containing details about a group.
		/// </summary>
		/// <param name="groupId">The id of the group to return.</param>
		/// <returns>The <see cref="GroupInfo"/> specified by the group id.</returns>
		public GroupInfo GroupsGetInfo(string groupId)
		{
			string url = _baseUrl + "?method=flickr.groups.getInfo&api_key=" + _apiKey + "&group_id=" + groupId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.GroupInfo;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}
		#endregion

		#region [ Group Pool ]
		/// <summary>
		/// Adds a photo to a pool you have permission to add photos to.
		/// </summary>
		/// <param name="photoId">The id of one of your photos to be added.</param>
		/// <param name="groupId">The id of a group you are a member of.</param>
		/// <returns>True on a successful addition.</returns>
		public bool GroupPoolAdd(string photoId, string groupId)
		{
			string url = _baseUrl + "?method=flickr.groups.pools.add&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&photo_id=" + photoId;
			url += "&group_id=" + groupId;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Remove a picture from a group.
		/// </summary>
		/// <param name="photoId">The id of one of your pictures you wish to remove.</param>
		/// <param name="groupId">The id of the group to remove the picture from.</param>
		/// <returns>True if the photo is successfully removed.</returns>
		public bool GroupPoolRemove(string photoId, string groupId)
		{
			string url = _baseUrl + "?method=flickr.groups.pools.remove&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&photo_id=" + photoId;
			url += "&group_id=" + groupId;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Gets a list of 
		/// </summary>
		/// <returns></returns>
		public PoolGroups GroupPoolGetGroups()
		{
			string url = _baseUrl + "?method=flickr.groups.pools.getGroups&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.PoolGroups;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public Photos GroupPoolGetPhotos(string groupId)
		{
			return GroupPoolGetPhotos(groupId, null, 0, 0);
		}

		public Photos GroupPoolGetPhotos(string groupId, string tags)
		{
			return GroupPoolGetPhotos(groupId, tags, 0, 0);
		}

		public Photos GroupPoolGetPhotos(string groupId, int perPage, int page)
		{
			return GroupPoolGetPhotos(groupId, null, perPage, page);
		}

		public Photos GroupPoolGetPhotos(string groupId, string tags, int perPage, int page)
		{
			string url = _baseUrl + "?method=flickr.groups.pools.getPhotos&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&group_id=" + groupId;
			if( tags != null ) url += "&tags=" + tags;
			if( perPage > 0 ) url += "&per_page=" + perPage;
			if( page > 0 ) url += "&page=" + page;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photos;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}
		#endregion

		#region [ People ]
		public User PeopleFindByEmail(string emailAddress)
		{
			string url = _baseUrl + "?method=flickr.people.findByEmail&api_key=" + _apiKey;
			url += "&find_email=" + Utils.UrlEncode(emailAddress);

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.User;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Returns a <see cref="User"/> object matching the screen name.
		/// </summary>
		/// <param name="username">The screen name or username of the user.</param>
		/// <returns>A <see cref="User"/> class containing the userId and username of the user.</returns>
		/// <exception cref="FlickrException">Thrown when user is not found.</exception>
		public User PeopleFindByUsername(string username)
		{
			string url = _baseUrl + "?method=flickr.people.findByUsername&api_key=" + _apiKey;
			url += "&username=" + Utils.UrlEncode(username);

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.User;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public Person PeopleGetInfo(string userId)
		{
			string url = _baseUrl + "?method=flickr.people.getInfo&api_key=" + _apiKey + "&user_id=" + userId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Person;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public Online PeopleGetOnlineList()
		{
			string url = _baseUrl + "?method=flickr.people.getOnlineList&api_key=" + _apiKey;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.OnlineUsers;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public Photos PeopleGetPublicPhotos(string userId)
		{
			string url = _baseUrl + "?method=flickr.people.getPublicPhotos&api_key=" + _apiKey + "&user_id=" + userId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photos;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}
		#endregion

		#region [ Photos ]
		public bool PhotosAddTags(string photoId, string[] tags)
		{	
			string s = string.Join(",", tags);
			return PhotosAddTags(photoId, s);
		}

		public bool PhotosAddTags(string photoId, string tags)
		{
			string url = _baseUrl + "?method=flickr.photos.addTags&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&photo_id=" + photoId;
			url += "&tags=" + Utils.UrlEncode(tags);

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public Photos PhotosGetContactsPhotos()
		{
			return PhotosGetContactsPhotos(0, false, false, false);
		}

		public Photos PhotosGetContactsPhotos(long count)
		{
			return PhotosGetContactsPhotos(count, false, false, false);
		}

		public Photos PhotosGetContactsPhotos(long count, bool justFriends, bool singlePhoto, bool includeSelf)
		{
			string url = _baseUrl + "?method=flickr.photos.getContactsPhotos&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			if( count > 0 ) url += "&count=" + count;
			if( justFriends ) url += "&just_friends=1";
			if( singlePhoto ) url += "&single_photo=1";
			if( includeSelf ) url += "&include_self=1";

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photos;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public Photos PhotosGetContactsPublicPhotos(string userId)
		{
			return PhotosGetContactsPublicPhotos(userId, 0, false, false, false);
		}

		public Photos PhotosGetContactsPublicPhotos(string userId, long count)
		{
			return PhotosGetContactsPublicPhotos(userId, count, false, false, false);
		}

		public Photos PhotosGetContactsPublicPhotos(string userId, long count, bool justFriends, bool singlePhoto, bool includeSelf)
		{
			string url = _baseUrl + "?method=flickr.photos.getContactsPublicPhotos&api_key=" + _apiKey + "&user_id=" + userId;
			if( count > 0 ) url += "&count=" + count;
			if( justFriends ) url += "&just_friends=1";
			if( singlePhoto ) url += "&single_photo=1";
			if( includeSelf ) url += "&include_self=1";

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photos;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Returns count of photos between each pair of dates in the list.
		/// </summary>
		/// <remarks>If you pass in DateA, DateB and DateC it returns
		/// a list of the number of photos between DateA and DateB,
		/// followed by the number between DateB and DateC. 
		/// More parameters means more sets.</remarks>
		/// <param name="dates">Array of <see cref="DateTime"/> objects.</param>
		/// <returns><see cref="PhotoCounts"/> class instance.</returns>
		public PhotoCounts PhotosGetCounts(DateTime[] dates)
		{
			string s = "";
			foreach(DateTime d in dates)
			{
				s += Utils.DateToUnixTimestamp(d) + ",";
			}
			if( s.Length > 0 ) s = s.Substring(0, s.Length - 1);

			return PhotosGetCounts(s);
		}

		public PhotoCounts PhotosGetCounts(string dates)
		{
			string url = _baseUrl + "?method=flickr.photos.getCounts&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password + "&dates=" + dates;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.PhotoCounts;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public PhotoInfo PhotosGetInfo(string photoId)
		{
			string url = _baseUrl + "?method=flickr.photos.getInfo&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password + "&photo_id=" + photoId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.PhotoInfo;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public PhotoPermissions PhotosGetPerms(string photoId)
		{
			string url = _baseUrl + "?method=flickr.photos.getPerms&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password + "&photo_id=" + photoId;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.PhotoPermissions;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public Photos PhotosGetRecent()
		{
			return PhotosGetRecent(0, 0);
		}

		public Photos PhotosGetRecent(long perPage, long page)
		{
			string url = _baseUrl + "?method=flickr.photos.getRecent&api_key=" + _apiKey;
			if( perPage > 0 ) url += "&per_page=" + perPage;
			if( page > 0 ) url += "&page=" + page;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photos;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public Sizes PhotosGetSizes(string photoId)
		{
			string url = _baseUrl + "?method=flickr.photos.getSizes&api_key=" + _apiKey + "&photo_id=" + photoId;

			FlickrNet.Response response = GetResponseAlwaysCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Sizes;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public Photos PhotosGetUntagged()
		{
			return PhotosGetUntagged(0, 0);
		}

		public Photos PhotosGetUntagged(int perPage, int page)
		{
			string url = _baseUrl + "?method=flickr.photos.getUntagged&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			if( perPage > 0 ) url += "&per_page=" + perPage;
			if( page > 0 ) url += "&page=" + page;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photos;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Gets a list of all current licenses.
		/// </summary>
		/// <returns><see cref="Licenses"/> instance.</returns>
		public Licenses PhotosLicensesGetInfo()
		{
			string url = _baseUrl + "?method=flickr.photos.licenses.getInfo&api_key=" + _apiKey;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Licenses;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Remove an existing tag.
		/// </summary>
		/// <param name="tagId">The id of the tag, as returned by <see cref="Flickr.PhotosGetInfo"/> or similar method.</param>
		/// <returns>True if the tag was removed.</returns>
		public bool PhotosRemoveTag(long tagId)
		{
			string url = _baseUrl + "?method=flickr.photos.removeTag&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&tag_id=" + tagId;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		// PhotoSearch - text versions
		public Photos PhotosSearchText(string userId, string text)
		{
			return PhotosSearch(userId, "", 0, text, DateTime.MinValue, DateTime.MinValue, 0, 0, 0);
		}

		public Photos PhotosSearchText(string userId, string text, int license)
		{
			return PhotosSearch(userId, "", 0, text, DateTime.MinValue, DateTime.MinValue, license, 0, 0);
		}

		public Photos PhotosSearchText(string text)
		{
			return PhotosSearch(null, "", 0, text, DateTime.MinValue, DateTime.MinValue, 0, 0, 0);
		}

		public Photos PhotosSearchText(string text, int license)
		{
			return PhotosSearch(null, "", 0, text, DateTime.MinValue, DateTime.MinValue, license, 0, 0);
		}

		// PhotoSearch - tag array versions
		public Photos PhotosSearch(string[] tags)
		{
			return PhotosSearch(null, tags, 0, "", DateTime.MinValue, DateTime.MinValue, 0, 0, 0);
		}

		public Photos PhotosSearch(string[] tags, int license)
		{
			return PhotosSearch(null, tags, 0, "", DateTime.MinValue, DateTime.MinValue, license, 0, 0);
		}

		public Photos PhotosSearch(string[] tags, TagMode tagMode, string text, int perPage, int page)
		{
			return PhotosSearch(null, tags, tagMode, text, DateTime.MinValue, DateTime.MinValue, 0, perPage, page);
		}

		public Photos PhotosSearch(string[] tags, TagMode tagMode, string text)
		{
			return PhotosSearch(null, tags, tagMode, text, DateTime.MinValue, DateTime.MinValue, 0, 0, 0);
		}

		public Photos PhotosSearch(string userId, string[] tags)
		{
			return PhotosSearch(userId, tags, 0, "", DateTime.MinValue, DateTime.MinValue, 0, 0, 0);
		}

		public Photos PhotosSearch(string userId, string[] tags, int license)
		{
			return PhotosSearch(userId, tags, 0, "", DateTime.MinValue, DateTime.MinValue, license, 0, 0);
		}

		public Photos PhotosSearch(string userId, string[] tags, TagMode tagMode, string text, int perPage, int page)
		{
			return PhotosSearch(userId, tags, tagMode, text, DateTime.MinValue, DateTime.MinValue, 0, perPage, page);
		}

		public Photos PhotosSearch(string userId, string[] tags, TagMode tagMode, string text)
		{
			return PhotosSearch(userId, tags, tagMode, text, DateTime.MinValue, DateTime.MinValue, 0, 0, 0);
		}

		public Photos PhotosSearch(string userId, string[] tags, TagMode tagMode, string text, DateTime minUploadDate, DateTime maxUploadDate, int license, int perPage, int page)
		{
			return PhotosSearch(userId, String.Join(",", tags), tagMode, text, minUploadDate, maxUploadDate, license, perPage, page);
		}

		// PhotoSearch - tags versions
		public Photos PhotosSearch(string tags, int license)
		{
			return PhotosSearch(null, tags, 0, "", DateTime.MinValue, DateTime.MinValue, license, 0, 0);
		}

		public Photos PhotosSearch(string tags, TagMode tagMode, string text, int perPage, int page)
		{
			return PhotosSearch(null, tags, tagMode, text, DateTime.MinValue, DateTime.MinValue, 0, perPage, page);
		}

		public Photos PhotosSearch(string tags, TagMode tagMode, string text)
		{
			return PhotosSearch(null, tags, tagMode, text, DateTime.MinValue, DateTime.MinValue, 0, 0, 0);
		}

		public Photos PhotosSearch(string userId, string tags)
		{
			return PhotosSearch(userId, tags, 0, "", DateTime.MinValue, DateTime.MinValue, 0, 0, 0);
		}

		public Photos PhotosSearch(string userId, string tags, int license)
		{
			return PhotosSearch(userId, tags, 0, "", DateTime.MinValue, DateTime.MinValue, license, 0, 0);
		}

		public Photos PhotosSearch(string userId, string tags, TagMode tagMode, string text, int perPage, int page)
		{
			return PhotosSearch(userId, tags, tagMode, text, DateTime.MinValue, DateTime.MinValue, 0, perPage, page);
		}

		public Photos PhotosSearch(string userId, string tags, TagMode tagMode, string text)
		{
			return PhotosSearch(userId, tags, tagMode, text, DateTime.MinValue, DateTime.MinValue, 0, 0, 0);
		}

		// Actual PhotoSearch function
		public Photos PhotosSearch(string userId, string tags, TagMode tagMode, string text, DateTime minUploadDate, DateTime maxUploadDate, int license, int perPage, int page)
		{
			string url = _baseUrl + "?method=flickr.photos.search&api_key=" + _apiKey;

			if( userId != null )
				url += "&user_id=" + userId;
			if( tags != null && tags.Length > 0 )
				url += "&tags=" + Utils.UrlEncode(tags);
			if( tagMode != 0 ) 
				url += "&tag_mode=" + (tagMode==TagMode.AllTags?"any":"all");
			if( text != null && text.Length > 0 )
				url += "&text=" + Utils.UrlEncode(text);
			if( minUploadDate != DateTime.MinValue )
				url += "&min_upload_date=" + minUploadDate.ToString();
			if( maxUploadDate != DateTime.MinValue )
				url += "&max_upload_date=" + maxUploadDate.ToString();
			if( license != 0 )
				url += "&license=" + (int)license;
			if( perPage != 0 )
				url += "&per_page=" + perPage;
			if( page != 0 )
				url += "&page=" + page;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photos;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Sets the title and description of the photograph.
		/// </summary>
		/// <param name="photoId">The numerical photoId of the photograph.</param>
		/// <param name="title">The new title of the photograph.</param>
		/// <param name="description">The new description of the photograph.</param>
		/// <returns>True when the operation is successful.</returns>
		/// <exception cref="FlickrException">Thrown when the photo id cannot be found.</exception>
		public bool PhotosSetMeta(string photoId, string title, string description)
		{
			string url = _baseUrl + "?method=flickr.photos.setMeta&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&photo_id=" + photoId;
			url += "&title=" + Utils.UrlEncode(title);
			url += "&description=" + Utils.UrlEncode(description);

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}

		}

		public bool PhotosSetPerms(string photoId, int isPublic, int isFriend, int isFamily, PermissionComment permComment, PermissionAddMeta permAddMeta)
		{
			return PhotosSetPerms(photoId, (isPublic==1), (isFriend==1), (isFamily==1), permComment, permAddMeta);
		}

		public bool PhotosSetPerms(string photoId, bool isPublic, bool isFriend, bool isFamily, PermissionComment permComment, PermissionAddMeta permAddMeta)
		{
			string url = _baseUrl + "?method=flickr.photos.setPerms&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&photo_id=" + photoId;
			url += "&is_public=" + (isPublic?"1":"0");
			url += "&is_friend=" + (isFriend?"1":"0");
			url += "&is_family=" + (isFamily?"1":"0");
			url += "&perm_comment=" + (int)permComment;
			url += "&perm_addmeta=" + (int)permAddMeta;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}

		}

		public bool PhotosSetTags(string photoId, string[] tags)
		{
			string s = string.Join(",", tags);
			return PhotosSetTags(photoId, s);
		}
			
		public bool PhotosSetTags(string photoId, string tags)
		{
			string url = _baseUrl + "?method=flickr.photos.setTags&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&photo_id=" + photoId;
			url += "&tags=" + tags;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}

		}
		#endregion

		#region [ Photosets ]
		public Photoset PhotosetsCreate(string title, string primaryPhotoId)
		{
			return PhotosetsCreate(title, null, primaryPhotoId);
		}

		public Photoset PhotosetsCreate(string title, string description, string primaryPhotoId)
		{
			string url = _baseUrl + "?method=flickr.photosets.create&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&title=" + Utils.UrlEncode(title);
			if( description != null ) url += "&description=" + Utils.UrlEncode(description);
			url += "&primary_photo_id=" + primaryPhotoId;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photoset;
			}
			else
			{
				throw new FlickrException(response.Error);
			}

		}

		public bool PhotosetsDelete(string photsetId)
		{
			string url = _baseUrl + "?method=flickr.photosets.delete&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&photoset_id=" + photsetId;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}

		}

		public bool PhotosetsEditMeta(string photosetId, string title, string description)
		{
			string url = _baseUrl + "?method=flickr.photosets.editMeta&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&photoset_id=" + photosetId;
			url += "&title=" + title;
			url += "&description=" + description;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}

		}

		public bool PhotosetsEditPhotos(string photosetId, string primaryPhotoId, string[] photoIds)
		{
			return PhotosetsEditPhotos(photosetId, primaryPhotoId, string.Join(",", photoIds));
		}

		public bool PhotosetsEditPhotos(string photosetId, string primaryPhotoId, string photoIds)
		{
			string url = _baseUrl + "?method=flickr.photosets.editPhotos&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&photoset_id=" + photosetId;
			url += "&primary_photo_id=" + primaryPhotoId;
			url += "&photo_ids=" + photoIds;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}

		}

		public Photoset PhotosetsGetInfo(string photosetId)
		{
			string url = _baseUrl + "?method=flickr.photosets.getInfo&api_key=" + _apiKey;
			url += "&photoset_id=" + photosetId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photoset;
			}
			else
			{
				throw new FlickrException(response.Error);
			}

		}

		public Photosets PhotosetsGetList(string userId)
		{
			string url = _baseUrl + "?method=flickr.photosets.getList&api_key=" + _apiKey;
			url += "&user_id=" + userId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photosets;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public Photoset PhotosetsGetPhotos(string photosetId)
		{
			string url = _baseUrl + "?method=flickr.photosets.getPhotos&api_key=" + _apiKey;
			url += "&photoset_id=" + photosetId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Photoset;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public bool PhotosetsOrderSets(string[] photosetIds)
		{
			return PhotosetsOrderSets(string.Join(",", photosetIds));
		}

		public bool PhotosetsOrderSets(string photosetIds)
		{
			string url = _baseUrl + "?method=flickr.photosets.orderSets&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			url += "&photoset_ids=" + photosetIds;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return true;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}
		#endregion

		#region [ Tags ]
		public PhotoInfo TagsGetListPhoto(string photoId)
		{
			string url = _baseUrl + "?method=flickr.tags.getListPhoto&api_key=" + _apiKey + "&photo_id=" + photoId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.PhotoInfo;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public WhoInfo TagsGetListUser()
		{
			return TagsGetListUser(null);
		}

		public WhoInfo TagsGetListUser(string userId)
		{
			string url = _baseUrl + "?method=flickr.tags.getListUser&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			if( userId != null ) url += "&user_id=" + userId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Who;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public WhoInfo TagsGetListUserPopular()
		{
			return TagsGetListUserPopular(null, 0);
		}
			
		public WhoInfo TagsGetListUserPopular(int count)
		{
			return TagsGetListUserPopular(null, count);
		}
			
		public WhoInfo TagsGetListUserPopular(string userId)
		{
			return TagsGetListUserPopular(userId, 0);
		}
			
		public WhoInfo TagsGetListUserPopular(string userId, long count)
		{
			string url = _baseUrl + "?method=flickr.tags.getListUserPopular&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;
			if( userId != null ) url += "&user_id=" + userId;
			if( count > 0 ) url += "&count=" + count;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.Who;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		#endregion

		#region [ Tests ]
		/// <summary>
		/// Runs the flickr.test.echo method and returned an array of <see cref="XmlElement"/> items.
		/// </summary>
		/// <param name="echoText">The text to pass to the method. If not empty should start with a '&amp;'.</param>
		/// <returns>An array of <see cref="XmlElement"/> items.</returns>
		/// <remarks>
		/// The APi Key has been removed from the returned array and will not be shown.
		/// </remarks>
		/// <example>
		/// XmlElement[] elements = flickr.TestEcho("&amp;param=value");
		/// foreach(XmlElement element in elements)
		/// {
		///		if( element.Name = "method" )
		///			Console.WriteLine("Method = " + element.InnerXml);
		///		if( element.Name = "param" )
		///			Console.WriteLine("Param = " + element.InnerXml);
		/// }
		/// </example>
		public XmlElement[] TestEcho(string echoText)
		{
			string url = _baseUrl + "?method=flickr.test.echo&api_key=" + _apiKey + echoText;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				// Remove the api_key element from the array.
				XmlElement[] elements = new XmlElement[response.AllElements.Length - 1];
				int c = 0;
				foreach(XmlElement element in response.AllElements)
				{
					if(element.Name != "api_key" )
						elements[c++] = element;
				}
				return elements;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		/// <summary>
		/// Test the logged in state of the current Filckr object.
		/// </summary>
		/// <returns>The <see cref="User"/> object containing the username and userid of the current user.</returns>
		public User TestLogin()
		{
			string url = _baseUrl + "?method=flickr.test.login&api_key=" + _apiKey + "&email=" + _email + "&password=" + _password;

			FlickrNet.Response response = GetResponseNoCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.User;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}
		#endregion

		#region [ Urls ]
		public GroupInfo UrlsGetGroup(string groupId)
		{
			string url = _baseUrl + "?method=flickr.urls.getGroup&api_key=" + _apiKey + "&group_id=" + groupId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.GroupInfo;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public User UrlsGetUserPhotos(string userId)
		{
			string url = _baseUrl + "?method=flickr.urls.getUserPhotos&api_key=" + _apiKey + "&user_id=" + userId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.User;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}
		
		public User UrlsGetUserProfile(string userId)
		{
			string url = _baseUrl + "?method=flickr.urls.getUserProfile&api_key=" + _apiKey + "&user_id=" + userId;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.User;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public GroupInfo UrlsLookupGroup(string urlToFind)
		{
			string url = _baseUrl + "?method=flickr.urls.lookupGroup&api_key=" + _apiKey + "&url=" + urlToFind;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.GroupInfo;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}

		public User UrlsLookupUser(string urlToFind)
		{
			string url = _baseUrl + "?method=flickr.urls.lookupUser&api_key=" + _apiKey + "&url=" + urlToFind;

			FlickrNet.Response response = GetResponseCache(url);

			if( response.Status == ResponseStatus.OK )
			{
				return response.User;
			}
			else
			{
				throw new FlickrException(response.Error);
			}
		}
		#endregion
	}

	public enum TagMode
	{
		AnyTag,
		AllTags
	}

}
