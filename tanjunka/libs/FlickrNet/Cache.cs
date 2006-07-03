using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections;

namespace FlickrNet
{
	/// <summary>
	/// Summary description for Cache.
	/// </summary>
	internal sealed class Cache
	{
		private Cache()
		{
		}

		static Cache()
		{
		
			LoadCache();

			_timer = new System.Timers.Timer();
			_timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
			_timer.Interval = 10 * 60 * 1000; // 10 minutes
			_timer.Start();
		}

		private static System.Timers.Timer _timer = null;

		internal static void DeleteOldest()
		{
			if( DownloadCache.Count == 0 ) return;

			PictureCacheItem itm = (PictureCacheItem)DownloadCache.GetByIndex(0);
			System.Diagnostics.Debug.WriteLine("Delete url: " + itm.url);
			CacheSize -= itm.fileSize;
			if( File.Exists(itm.filename) ) File.Delete(itm.filename);
			
			DownloadCache.RemoveAt(0);

			return;
		}

		internal static long CacheSizeLimit
		{
			get 
			{
				if( CacheSettings.ContainsKey("SizeLimit") )
					return (long)CacheSettings["SizeLimit"];
				else
					return 50 * 1024 * 1024;
			}
			set 
			{ 
				if( CacheSettings.ContainsKey("SizeLimit") )
					CacheSettings["SizeLimit"] = value;
				else
					CacheSettings.Add("SizeLimit", value);
			}
		}

		internal static long CacheSize
		{
			get 
			{
				if( CacheSettings.ContainsKey("CurrentSize") )
					return (long)CacheSettings["CurrentSize"];
				else
					return 0;
			}
			set 
			{ 
				if( CacheSettings.ContainsKey("CurrentSize") )
					CacheSettings["CurrentSize"] = value;
				else
					CacheSettings.Add("CurrentSize", value);
			}
		}


		private static TimeSpan _cachetimeout = new TimeSpan(0, 0, 10);

		public static TimeSpan CacheTimeout
		{
			get { return _cachetimeout; }
			set { _cachetimeout = value; }
		}
		
		private static object _lockObject = new object();

		public static SortedList ResponseCache = new SortedList();
		public static SortedList DownloadCache = new SortedList();
		private static Hashtable CacheSettings = new Hashtable();

		[Serializable]
		internal struct ResponseCacheItem
		{
			public string url;
			public Response response;
			public DateTime creationTime;
		}

		[Serializable]
		internal struct PictureCacheItem
		{
			public string url;
			public DateTime creationTime;
			public string filename;
			public long fileSize;
		}

		private static void SaveCache()
		{
			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			System.IO.Stream stream = Utils.GetApplicationDataWriteStream("responseCache.bin");
			formatter.Serialize(stream, ResponseCache);
			stream.Close();

			stream = Utils.GetApplicationDataWriteStream("downloadCache.bin");
			formatter.Serialize(stream, DownloadCache);
			stream.Close();

			stream = Utils.GetApplicationDataWriteStream("cacheSettings.bin");
			formatter.Serialize(stream, CacheSettings);
			stream.Close();
		}

		private static void LoadCache()
		{
			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

			System.IO.Stream stream = Utils.GetApplicationDataReadStream("cacheSettings.bin");
			if( stream == null )
			{
				CacheSettings = new Hashtable();
				return;
			}
			try
			{
				CacheSettings = (Hashtable)formatter.Deserialize(stream);
			}
			catch
			{
				CacheSettings = new Hashtable();
			}
			stream.Close();

			stream = Utils.GetApplicationDataReadStream("responseCache.bin");
			if( stream == null )
			{
				ResponseCache = new SortedList();
				return;
			}
			try
			{
				ResponseCache = (SortedList)formatter.Deserialize(stream);
			}
			catch
			{
				ResponseCache = new SortedList();
			}
			stream.Close();

			stream = Utils.GetApplicationDataReadStream("downloadCache.bin");
			if( stream == null )
			{
				DownloadCache = new SortedList();
				return;
			}
			try
			{
				DownloadCache = (SortedList)formatter.Deserialize(stream);
			}
			catch
			{
				DownloadCache = new SortedList();
			}
			stream.Close();

			return;
		}

		private static void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			ProcessSave();
		}

		private static void ProcessSave()
		{
//			foreach(Object o in ResponseCache.Values)
//			{
//				ResponseCacheItem item = (ResponseCacheItem)o;
//				if( item.creationTime + CacheTimeout > DateTime.Now )
//				{
//					ResponseCache.Remove(item.url);
//				}
//			}
//
//			foreach(Object o in DownloadCache.Values)
//			{
//				PictureCacheItem item = (PictureCacheItem)o;
//				if( item.creationTime + CacheTimeout > DateTime.Now )
//				{
//					DownloadCache.Remove(item.url);
//				}
//			}

			SaveCache();
		}

		internal static void ForceSave()
		{
			ProcessSave();
		}

		internal static void FlushCache(string url)
		{
			if( ResponseCache.ContainsKey(url) )
				ResponseCache.Remove(url);
			if( DownloadCache.ContainsKey(url) )
				ResponseCache.Remove(url);
		}

		internal static void FlushCache()
		{
			ResponseCache.Clear();
			foreach(Object o in DownloadCache.Values)
			{
				PictureCacheItem itm = (PictureCacheItem)o;
				Utils.DeleteISFFile(itm.filename);
			}
			DownloadCache.Clear();
		}
	}
}
