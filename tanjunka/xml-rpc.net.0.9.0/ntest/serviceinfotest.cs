using System;
using System.IO;
using System.Reflection;
using  NUnit.Framework;
using CookComputing.XmlRpc;

namespace ntest
{       
  [TestFixture]
  public class ServiceInfoTest
  {
    public void testInt32()
    {
      Type type = typeof(int);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals("Int32 doesn't map to XmlRpcType.tInt32", 
        XmlRpcType.tInt32, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("Int32 doesn't map to 'integer'", 
        rpcString, "integer");
    }
    
    public void testXmlRpcInt()
    {
      Type type = typeof(XmlRpcInt);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals("XmlRpcInt doesn't map to XmlRpcType.tInt32", 
        XmlRpcType.tInt32, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("XmlRpcInt doesn't map to 'integer'", 
        rpcString, "integer");
    }
    
    public void testBoolean()
    {
      Type type = typeof(bool);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals("Boolean doesn't map to XmlRpcType.tBoolean", 
        XmlRpcType.tBoolean, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("Boolean doesn't map to 'boolean'", 
        rpcString, "boolean");
    }

    public void testXmlRpcBoolean()
    {
      Type type = typeof(XmlRpcBoolean);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals("XmlRpcBoolean doesn't map to XmlRpcType.tBoolean", 
        XmlRpcType.tBoolean, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("XmlRpcBoolean doesn't map to 'boolean'", 
        rpcString, "boolean");
    }
    
    public void testString()
    {
      Type type = typeof(string);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals("String doesn't map to XmlRpcType.tString", 
        XmlRpcType.tString, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("String doesn't map to 'string'", 
        rpcString, "string");
    }
    
    public void testDouble()
    {
      Type type = typeof(double);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals("Double doesn't map to XmlRpcType.tDouble", 
        XmlRpcType.tDouble, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("Double doesn't map to 'double'", 
        rpcString, "double");
    }
    
    public void testXmlRpcDouble()
    {
      Type type = typeof(XmlRpcDouble);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals("XmlRpcDouble doesn't map to XmlRpcType.tDouble", 
        XmlRpcType.tDouble, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("XmlRpcDouble doesn't map to 'double'", 
        rpcString, "double");
    }
    
    public void testDateTime()
    {
      Type type = typeof(DateTime);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals("DateTime doesn't map to XmlRpcType.tDateTime", 
        XmlRpcType.tDateTime, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("DateTime doesn't map to 'dateTime'", 
        rpcString, "dateTime");
    }
    
    public void testXmlRpcDateTime()
    {
      Type type = typeof(XmlRpcDateTime);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals(
        "XmlRpcDateTime doesn't map to XmlRpcType.tDateTime", 
        XmlRpcType.tDateTime, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("XmlRpcDateTime doesn't map to 'dateTime'", 
        rpcString, "dateTime");
    }
    
    public void testBase64()
    {
      Type type = typeof(byte[]);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals("Byte[] doesn't map to XmlRpcType.tBase64", 
        XmlRpcType.tBase64, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("Byte[] doesn't map to 'base64'", 
        rpcString, "base64");
    }
    
    public void testXmlRpcStruct()
    {
      Type type = typeof(XmlRpcStruct);
        XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals(
        "XmlRpcStruct doesn't map to XmlRpcType.tHashtable", 
        XmlRpcType.tHashtable, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("XmlRpcStruct doesn't map to 'struct'", 
        rpcString, "struct");
    }
    
    public void testArray()
    {
      Type type = typeof(Array);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals(
        "Array doesn't map to XmlRpcType.tArray", 
        XmlRpcType.tArray, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("Array doesn't map to 'array'", 
        rpcString, "array");
    }

    public void testIntArray()
    {
      Type type = typeof(Int32[]);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals(
        "Int32[] doesn't map to XmlRpcType.tArray", 
        XmlRpcType.tArray, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("Int32[] doesn't map to 'array'", 
        rpcString, "array");
    }
    
    //TODO: add test for [][] etc
    
    public void testVoid()
    {
      Type type = typeof(void);
        XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals("void doesn't map to XmlRpcType.tVoid", 
        XmlRpcType.tVoid, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("void doesn't map to 'void'", 
        rpcString, "void");
    }
    
    struct struct1
    {
      public int mi;
    }
    
    public void testStruct()
    {
      Type type = typeof(struct1);
      XmlRpcType rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
      Assertion.AssertEquals("struct doesn't map to XmlRpcType.tStruct", 
        XmlRpcType.tStruct, rpcType);
      string rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
      Assertion.AssertEquals("struct doesn't map to 'struct'", 
        rpcString, "struct");
    }
    
    
  }
}