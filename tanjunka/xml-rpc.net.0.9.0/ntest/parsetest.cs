using System;
using System.IO;
using System.Xml;
using System.Reflection;
using  NUnit.Framework;
using CookComputing.XmlRpc;

// TODO: parse array
// TODO: parse struct
// TODO: parse XmlRpcStruct
// TODO: parse XmlRpcStruct derived
// TODO: array of base64

namespace ntest
{
  [TestFixture]
  public class ParseTest
  {
    class Struct1 : XmlRpcStruct
    {
      public int mi;
    }

    struct Struct2
    {
      public int mi;
      public string ms;
      public bool mb;
      public double md;
      public DateTime mdt;
      public byte[] mb64;
      public int[] ma;
      public XmlRpcInt xi;
      public XmlRpcBoolean xb;
      public XmlRpcDouble xd;
      public XmlRpcDateTime xdt;
      public XmlRpcStruct xstr;
    }

    //---------------------- int -------------------------------------------// 
    public void testInt_NullType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?><value><int>12345</int></value>";
      object obj = Utils.Parse(xml, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(12345, (int)obj);
    }
      
    public void testInt_IntType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?><value><int>12345</int></value>";
      object obj = Utils.Parse(xml, typeof(int), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(12345, (int)obj);
    }
      
    public void testInt_ObjectType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?><value><int>12345</int></value>";
      object obj = Utils.Parse(xml, typeof(object), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(12345, (int)obj);
    }
      
    //---------------------- string ----------------------------------------// 
    public void testString_NullType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><string>astring</string></value>";
      object obj = Utils.Parse(xml, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals("astring", (string)obj);
    }
      
    public void testDefaultString_NullType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value>astring</value>";
      object obj = Utils.Parse(xml, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals("astring", (string)obj);
    }

    public void testString_StringType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><string>astring</string></value>";
      object obj = Utils.Parse(xml, typeof(string), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals("astring", (string)obj);
    }
      
    public void testString_ObjectType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><string>astring</string></value>";
      object obj = Utils.Parse(xml, typeof(object), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals("astring", (string)obj);
    }
      
    public void testDefaultString_StringType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value>astring</value>";
      object obj = Utils.Parse(xml, typeof(string), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals("astring", (string)obj);
    }

    public void testEmpty1String_StringType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><string></string></value>";
      object obj = Utils.Parse(xml, typeof(string), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals("", (string)obj);
    }
      
    public void testEmpty2String_StringType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><string/></value>";
      object obj = Utils.Parse(xml, typeof(string), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals("", (string)obj);
    }
      
    public void testDefault1EmptyString_StringType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value></value>";
      object obj = Utils.Parse(xml, typeof(string), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals("", (string)obj);
    }

    public void testDefault2EmptyString_StringType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value/>";
      object obj = Utils.Parse(xml, typeof(string), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals("", (string)obj);
    }

    //---------------------- boolean ---------------------------------------// 
    public void testBoolean_NullType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><boolean>1</boolean></value>";
      object obj = Utils.Parse(xml, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(true, (bool)obj);
    }
      
    public void testBoolean_BooleanType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><boolean>1</boolean></value>";
      object obj = Utils.Parse(xml, typeof(bool), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(true, (bool)obj);
    }
      
    public void testBoolean_ObjectType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><boolean>1</boolean></value>";
      object obj = Utils.Parse(xml, typeof(object), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(true, (bool)obj);
    }
      
    //---------------------- double ----------------------------------------// 
    public void testDouble_NullType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><double>543.21</double></value>";
      object obj = Utils.Parse(xml, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(543.21, (double)obj);
    }
      
    public void testDouble_DoubleType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><double>543.21</double></value>";
      object obj = Utils.Parse(xml, typeof(double), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(543.21, (double)obj);
    }
      
    public void testDouble_ObjectType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><double>543.21</double></value>";
      object obj = Utils.Parse(xml, typeof(double), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(543.21, (double)obj);
    }
      
    //---------------------- dateTime ------------------------------------// 
    public void testDateTime_NullType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37</dateTime.iso8601></value>";
      object obj = Utils.Parse(xml, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(new DateTime(2002, 7, 6, 11, 25, 37), 
        (DateTime)obj);
    }
      
    public void testDateTime_DateTimeType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37</dateTime.iso8601></value>";
      object obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(new DateTime(2002, 7, 6, 11, 25, 37), 
        (DateTime)obj);
    }
      
    public void testDateTime_ObjectTimeType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37</dateTime.iso8601></value>";
      object obj = Utils.Parse(xml, typeof(object), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(new DateTime(2002, 7, 6, 11, 25, 37), 
        (DateTime)obj);
    }

    public void testDateTime_ROCA()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>2002-07-06T11:25:37</dateTime.iso8601></value>";
      object obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(new DateTime(2002, 7, 6, 11, 25, 37), 
        (DateTime)obj);
    }
      






    //---------------------- base64 ----------------------------------------// 
    byte[] testb = new Byte[] 
        {
            121, 111, 117, 32, 99, 97, 110, 39, 116, 32, 114, 101, 97, 100, 
          32, 116, 104, 105, 115, 33 };

    public void testBase64_NullType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><base64>eW91IGNhbid0IHJlYWQgdGhpcyE=</base64></value>";
      object obj = Utils.Parse(xml, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is array of byte", obj is byte[]);
      byte[] ret = obj as byte[];
      Assertion.Assert(ret.Length == testb.Length);
      for (int i = 0; i < testb.Length; i++)
        Assertion.Assert(testb[i] == ret[i]);
    }
  
    public void testBase64_Base64Type()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><base64>eW91IGNhbid0IHJlYWQgdGhpcyE=</base64></value>";
      object obj = Utils.Parse(xml, typeof(byte[]), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is array of byte", obj is byte[]);
      byte[] ret = obj as byte[];
      Assertion.Assert(ret.Length == testb.Length);
      for (int i = 0; i < testb.Length; i++)
        Assertion.Assert(testb[i] == ret[i]);
    }

    public void testBase64_ObjectType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><base64>eW91IGNhbid0IHJlYWQgdGhpcyE=</base64></value>";
      object obj = Utils.Parse(xml, typeof(object), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is array of byte", obj is byte[]);
      byte[] ret = obj as byte[];
      Assertion.Assert(ret.Length == testb.Length);
      for (int i = 0; i < testb.Length; i++)
        Assertion.Assert(testb[i] == ret[i]);
    }
  
    //---------------------- array -----------------------------------------// 
    public void testMixedArray_NullType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><string>Egypt</string></value>
      <value><boolean>0</boolean></value>
    </data>
  </array>
</value>";
      object obj = Utils.Parse(xml, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is array of object", obj is object[]);
      object[] ret = obj as object[];
      Assertion.AssertEquals(12, ret[0]);
      Assertion.AssertEquals("Egypt", ret[1]);
      Assertion.AssertEquals(false, ret[2]);
    }

    public void testMixedArray_ObjectArrayType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><string>Egypt</string></value>
      <value><boolean>0</boolean></value>
    </data>
  </array>
</value>";
      object obj = Utils.Parse(xml, typeof(object[]), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is array of object", obj is object[]);
      object[] ret = obj as object[];
      Assertion.AssertEquals(12, ret[0]);
      Assertion.AssertEquals("Egypt", ret[1]);
      Assertion.AssertEquals(false, ret[2]);
    }

    public void testMixedArray_ObjectType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><string>Egypt</string></value>
      <value><boolean>0</boolean></value>
    </data>
  </array>
</value>";
      object obj = Utils.Parse(xml, typeof(object), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is array of object", obj is object[]);
      object[] ret = obj as object[];
      Assertion.AssertEquals(12, ret[0]);
      Assertion.AssertEquals("Egypt", ret[1]);
      Assertion.AssertEquals(false, ret[2]);
    }
      
    public void testHomogArray_NullType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><i4>13</i4></value>
      <value><i4>14</i4></value>
    </data>
  </array>
</value>";
      object obj = Utils.Parse(xml, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is array of int", obj is int[]);
      int[] ret = obj as int[];
      Assertion.AssertEquals(12, ret[0]);
      Assertion.AssertEquals(13, ret[1]);
      Assertion.AssertEquals(14, ret[2]);
    }

    public void testHomogArray_IntArrayType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><i4>13</i4></value>
      <value><i4>14</i4></value>
    </data>
  </array>
</value>";
      object obj = Utils.Parse(xml, typeof(int[]), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is array of int", obj is int[]);
      int[] ret = obj as int[];
      Assertion.AssertEquals(12, ret[0]);
      Assertion.AssertEquals(13, ret[1]);
      Assertion.AssertEquals(14, ret[2]);
    }

    public void testHomogArray_ObjectArrayType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><i4>13</i4></value>
      <value><i4>14</i4></value>
    </data>
  </array>
</value>";
      object obj = Utils.Parse(xml, typeof(object[]), MappingAction.Error, 
      out parsedType, out parsedArrayType);
      Assertion.Assert("result is array of object", obj is object[]);
      object[] ret = obj as object[];
      Assertion.AssertEquals(12, ret[0]);
      Assertion.AssertEquals(13, ret[1]);
      Assertion.AssertEquals(14, ret[2]);
    }
    public void testHomogArray_ObjectType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><i4>13</i4></value>
      <value><i4>14</i4></value>
    </data>
  </array>
</value>";
      object obj = Utils.Parse(xml, typeof(object), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is array of int", obj is int[]);
      int[] ret = obj as int[];
      Assertion.AssertEquals(12, ret[0]);
      Assertion.AssertEquals(13, ret[1]);
      Assertion.AssertEquals(14, ret[2]);
    }
      
    //---------------------- struct ----------------------------------------// 
    public void testStructDerived_NullType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
<value>
  <struct>
    <member>
      <name>mi</name>
      <value><i4>18</i4></value>
    </member>
  </struct>
</value>";
      object obj = Utils.Parse(xml, typeof(Struct1), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Struct1 struct1 = new Struct1();
      struct1.mi = 18;
      Assertion.Assert(obj is Struct1);
      Struct1 ret = obj as Struct1;
      Assertion.AssertEquals(ret.mi, 18);
      Assertion.AssertEquals("member is in hashtable", ret["mi"], 18);
    }
     
    //---------------------- XmlRpcInt -------------------------------------// 
    public void testXmlRpcInt_XmlRpcIntType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?><value><int>12345</int></value>";
      object obj = Utils.Parse(xml, typeof(XmlRpcInt), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(12345, (XmlRpcInt)obj);
    }

    //---------------------- XmlRpcBoolean ---------------------------------// 
    public void testXmlRpcBoolean_XmlRpcBooleanType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><boolean>1</boolean></value>";
      object obj = Utils.Parse(xml, typeof(XmlRpcBoolean), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(new XmlRpcBoolean(true), (XmlRpcBoolean)obj);
    }
            
    //---------------------- XmlRpcDouble ----------------------------------// 
    public void testXmlRpcDouble_XmlRpcDoubleType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><double>543.21</double></value>";
      object obj = Utils.Parse(xml, typeof(XmlRpcDouble), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(new XmlRpcDouble(543.21), (XmlRpcDouble)obj);
    }
      
    //---------------------- XmlRpcDateTime --------------------------------// 
    public void testXmlRpcDateTime_XmlRpcDateTimeType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37</dateTime.iso8601></value>";
      object obj = Utils.Parse(xml, typeof(XmlRpcDateTime), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(
        new XmlRpcDateTime(new DateTime(2002, 7, 6, 11, 25, 37)), 
        (XmlRpcDateTime)obj);
    }
     
    //---------------------- XmlRpcStruct Derived --------------------------// 
    public void testXmlRpcStructDerived_DerivedType()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
<value>
  <struct>
    <member>
      <name>mi</name>
      <value><i4>18</i4></value>
    </member>
  </struct>
</value>";
      object obj = Utils.Parse(xml, typeof(Struct1), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Struct1 struct1 = new Struct1();
      struct1.mi = 18;
      Assertion.Assert(obj is Struct1);
      Struct1 ret = obj as Struct1;
      Assertion.AssertEquals(ret.mi, 18);
      Assertion.AssertEquals("member is in hashtable", ret["mi"], 18);
    }
  }
}
