using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace FlickrNet
{
	/// <summary>
	/// Summary description for Utils.
	/// </summary>
	public sealed class Utils
	{
		private static readonly DateTime unixStartDate = new DateTime(1970, 1, 1, 0, 0, 0);

		private Utils()
		{
		}

		/// <summary>
		/// Encodes url items according to some specific rules, i.e. '&amp;', '=' and ' ' are also encoded to 
		/// their respective codes.
		/// </summary>
		/// <param name="oldString">The old url parameter value to be encoded</param>
		/// <returns>The encoded url paramter value.</returns>
		public static string UrlEncode(string oldString)
		{
			string a = System.Web.HttpUtility.UrlEncode(oldString);
			a = a.Replace("&", "%26");
			a = a.Replace("=", "%3D");
			a = a.Replace(" ", "%20");
			return a;
		}

		public static long DateToUnixTimestamp(DateTime theDate)
		{
			TimeSpan ts = theDate - unixStartDate;
			return (int)ts.TotalSeconds;
		}

		public static DateTime UnixTimestampToDate(string theTimestamp)
		{
			return UnixTimestampToDate(long.Parse(theTimestamp));
		}

		public static DateTime UnixTimestampToDate(long theTimestamp)
		{
			return unixStartDate.AddSeconds(theTimestamp);
		}

		internal static IsolatedStorageFile GetISF()
		{
			return IsolatedStorageFile.GetUserStoreForDomain();
		}

		internal static void DeleteISFFile(string path)
		{
			IsolatedStorageFile isf = GetISF();
			if( isf.GetFileNames(path).Length == 0 )
				return;

			isf.DeleteFile(path);
		}

		public static Stream GetISFWriteStream(string path)
		{
			IsolatedStorageFile isf = GetISF();

			IsolatedStorageFileStream stream = new IsolatedStorageFileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 1024, isf);

			return stream;
		}

		public static Stream GetISFReadStream(string path)
		{
			IsolatedStorageFile isf = GetISF();

			if( isf.GetFileNames(path).Length == 0 )
				return null;

			IsolatedStorageFileStream stream = new IsolatedStorageFileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024, isf);

			return stream;
		}

		public static Stream GetApplicationDataReadStream(string path)
		{
			string file = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\Flickr.Net\\", path);
			if( !File.Exists(file) ) return null;
			return File.OpenRead(file);
		}

		public static Stream GetApplicationDataWriteStream(string path)
		{
			string file = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\Flickr.Net\\", path);
			Directory.CreateDirectory(Path.GetDirectoryName(file));
			return new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
		}

		public static void DeleteAPplicationDataFile(string path)
		{
			string file = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\Flickr.Net\\", path);
			if( !File.Exists(file) ) return;
			File.Delete(file);
		}

	}
}
