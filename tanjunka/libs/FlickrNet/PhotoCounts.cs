using System;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace FlickrNet
{
	[System.Serializable]
	public class PhotoCounts
	{
		/// <remarks/>
		[XmlElement("photocount", Form=XmlSchemaForm.Unqualified)]
		public PhotoCountInfo[] PhotoCountInfoCollection = new PhotoCountInfo[0];
	}

	[System.Serializable]
	public class PhotoCountInfo
	{
		/// <summary>Total number of photos between the FromDate and the ToDate.</summary>
		/// <remarks/>
		[XmlAttribute("count", Form=XmlSchemaForm.Unqualified)]
		public int PhotoCount;
    
		/// <summary>The From date.</summary>
		[XmlIgnore()]
		public DateTime FromDate
		{
			get 
			{
				return Utils.UnixTimestampToDate(fromdate);
			}
		}

		/// <summary>The To date.</summary>
		[XmlIgnore()]
		public DateTime ToDate
		{
			get 
			{
				return Utils.UnixTimestampToDate(todate);
			}
		}

		/// <summary>The original from date in unix timestamp format.</summary>
		/// <remarks/>
		[XmlAttribute("fromdate", Form=XmlSchemaForm.Unqualified)]
		public string fromdate;
    
		/// <summary>The original to date in unix timestamp format.</summary>
		/// <remarks/>
		[XmlAttribute("todate", Form=XmlSchemaForm.Unqualified)]
		public string todate;

	}
}