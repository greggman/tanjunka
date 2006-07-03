using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using CookComputing.XmlRpc;

namespace ntest
{

  public class Utils
  {
    public static XmlDocument Serialize(
      string testName,
      object obj, 
      Encoding encoding,
      MappingAction action)
    {
      Stream stm = new MemoryStream();
      XmlTextWriter xtw = new XmlTextWriter(stm, Encoding.UTF8);
      xtw.Formatting = Formatting.Indented;
      xtw.Indentation = 2;
      xtw.WriteStartDocument();      
      XmlRpcSerializer ser = new XmlRpcSerializer();
      ser.Serialize(xtw, obj, MappingAction.Error); 
      xtw.Flush();
      //Console.WriteLine(testName);
      stm.Position = 0;    
      TextReader trdr = new StreamReader(stm, new UTF8Encoding(), true, 4096);
      String s = trdr.ReadLine();
      while (s != null)
      {
        //Console.WriteLine(s);
        s = trdr.ReadLine();
      }            
      stm.Position = 0;    
      XmlDocument xdoc = new XmlDocument();
      xdoc.Load(stm);
      return xdoc;
    }
    	
    public static string SerializeToString(
      string testName,
      object obj, 
      MappingAction action)
    {
      StringWriter strwrtr = new StringWriter();
      XmlTextWriter xtw = new XmlTextWriter(strwrtr);
//      xtw.Formatting = formatting;
//      xtw.Indentation = indentation;
      xtw.WriteStartDocument();      
      XmlRpcSerializer ser = new XmlRpcSerializer();
      ser.Serialize(xtw, obj, MappingAction.Error); 
      xtw.Flush();
      //Console.WriteLine(testName);
      //Console.WriteLine(strwrtr.ToString());
      return strwrtr.ToString();
    }

    //----------------------------------------------------------------------// 
    public static object Parse(
      string xml, 
      Type valueType, 
      MappingAction action,
      out Type parsedType,
      out Type parsedArrayType)
    {
      StringReader sr = new StringReader(xml);
      XmlDocument xdoc = new XmlDocument();
      xdoc.Load(sr);        
      return Parse(xdoc, valueType, action, 
        out parsedType, out parsedArrayType);
    }
    
    public static object Parse(
      XmlDocument xdoc, 
      Type valueType, 
      MappingAction action,
      out Type parsedType,
      out Type parsedArrayType)
    {
      XmlNode node = xdoc.SelectSingleNode("value").FirstChild;               
      XmlRpcSerializer.ParseStack parseStack 
        = new XmlRpcSerializer.ParseStack("request");
      XmlRpcSerializer ser = new XmlRpcSerializer();
      object obj = ser.ParseValue(node, valueType, parseStack, action,
        out parsedType, out parsedArrayType);
      return obj;
    }	
  }

}
