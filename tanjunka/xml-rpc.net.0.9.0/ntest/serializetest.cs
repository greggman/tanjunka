using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Threading;
using  NUnit.Framework;
using CookComputing.XmlRpc;
namespace ntest

// TODO: test any culture dependencies
{
  [TestFixture]
  public class SerializeTest
  {
    //---------------------- int -------------------------------------------// 
    public void testInt()
    {
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testInt", 
        12345, 
        Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;
      object obj = Utils.Parse(xdoc, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(12345, obj);
    }
  
    //---------------------- string ----------------------------------------// 
    public void testString()
    {
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testString", 
        "this is a string", 
        Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;
      object obj = Utils.Parse(xdoc, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals("this is a string", obj);
    }

    //---------------------- boolean ---------------------------------------// 
    public void testBoolean()
    {
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testBoolean", 
        true, 
        Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;
      object obj = Utils.Parse(xdoc, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(true, obj);
    }

    //---------------------- double ----------------------------------------// 
    public void testDouble()
    {
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testDouble", 
        543.21, 
        Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;
      object obj = Utils.Parse(xdoc, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(543.21, obj);
    }

    //---------------------- dateTime ------------------------------------// 
    public void testDateTime()
    {
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testDateTime", 
        new DateTime(2002, 7, 6, 11, 25, 37), 
        Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;
      object obj = Utils.Parse(xdoc, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(new DateTime(2002, 7, 6, 11, 25, 37), obj);
    }
    
    //---------------------- base64 ----------------------------------------// 
    public void testBase64()
    {
      byte[] testb = new Byte[] 
      {
        121, 111, 117, 32, 99, 97, 110, 39, 116, 32, 114, 101, 97, 100, 
        32, 116, 104, 105, 115, 33 
      };  
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testBase64", 
        testb, 
        Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;
      object obj = Utils.Parse(xdoc, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is array of byte", obj is byte[]);
      byte[] ret = obj as byte[];
      Assertion.Assert(ret.Length == testb.Length);
      for (int i = 0; i < testb.Length; i++)
        Assertion.Assert(testb[i] == ret[i]);
    }
    
    //---------------------- array -----------------------------------------// 
    public void testArray()
    {
      object[] testary = new Object[] { 12, "Egypt", false };
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testArray", 
      testary, 
      Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;    
      object obj = Utils.Parse(xdoc, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is array of object", obj is object[]);
      object[] ret = obj as object[];
      Assertion.AssertEquals(12, ret[0]);
      Assertion.AssertEquals("Egypt", ret[1]);
      Assertion.AssertEquals(false, ret[2]);    
    }    

    //---------------------- array -----------------------------------------// 
    public void testMultiDimArray()
    {
      int[,] myArray = new  int[,] {{1,2}, {3,4}};
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testMultiDimArray", 
        myArray, 
        Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;    
      object obj = Utils.Parse(xdoc, typeof(int[,]), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is 2 dim array of int", obj is int[,]);
      int[,] ret = obj as int[,];
      Assertion.AssertEquals(1, ret[0,0]);
      Assertion.AssertEquals(2, ret[0,1]);
      Assertion.AssertEquals(3, ret[1,0]);
      Assertion.AssertEquals(4, ret[1,1]);
    }    
      
    //---------------------- struct ----------------------------------------// 
    struct Struct1
    {
      public int mi;
      public string ms;
      public bool mb;
      public double md;
      public DateTime mdt;
      public byte[] mb64;
      public int[] ma;
      public bool Equals(Struct1 str)
      {
        if (mi != str.mi || ms != str.ms || md != str.md || mdt != str.mdt)
          return false;
        if (mb64.Length != str.mb64.Length)
          return false;
        for (int i = 0; i < mb64.Length; i++)
          if (mb64[i] != str.mb64[i])
            return false;
        for (int i = 0; i < ma.Length; i++)
          if (ma[i] != str.ma[i])
            return false;
        return true;
      }
    }
    
    public void testStruct()
    {
      byte[] testb = new Byte[] 
      {
        121, 111, 117, 32, 99, 97, 110, 39, 116, 32, 114, 101, 97, 100, 
        32, 116, 104, 105, 115, 33 
      }; 
      
      Struct1 str1 = new Struct1();
      str1.mi = 34567;
      str1.ms = "another test string";
      str1.mb = true;
      str1.md = 8765.123;
      str1.mdt = new DateTime(2002, 7, 6, 11, 25, 37);
      str1.mb64 = testb;
      str1.ma = new int[] { 1, 2, 3, 4, 5 };
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testStruct", 
        str1, 
        Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;    
      object obj = Utils.Parse(xdoc, typeof(Struct1), MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.Assert("result is Struct1", obj is Struct1);
      Struct1 str2 = (Struct1)obj;
      Assertion.Assert(str2.Equals(str1));
    }  

    struct Struct2
    {
      [XmlRpcMember("member_1")]
      public int member1;

      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public XmlRpcInt member2;

      [XmlRpcMember("member_3")]
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public XmlRpcInt member3;
    }

    public void testXmlRpcMember()
    {
      Stream stm = new MemoryStream();
      XmlRpcRequest req = new XmlRpcRequest();
      req.args = new Object[] { new Struct2() };
      req.method = "Foo";
      XmlRpcSerializer ser = new XmlRpcSerializer();
      ser.SerializeRequest(stm, req);
      stm.Position = 0;
      TextReader tr = new StreamReader(stm);
      string reqstr = tr.ReadToEnd();

      Assertion.AssertEquals(
@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>member_1</name>
            <value>
              <i4>0</i4>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
}




    struct Struct3
    {
      int _member1;
      public int member1 { get { return _member1; } set { _member1 = value; } } 

      int _member2;
      public int member2 { get { return _member2; }  } 

      int _member3;
      [XmlRpcMember("member-3")]
      public int member3 { get { return _member3; } set { _member3 = value; } } 

      int _member4;
      [XmlRpcMember("member-4")]
      public int member4 { get { return _member4; }  } 
    }

    public void testStructProperties()
    {
      Stream stm = new MemoryStream();
      XmlRpcRequest req = new XmlRpcRequest();
      req.args = new Object[] { new Struct3() };
      req.method = "Foo";
      XmlRpcSerializer ser = new XmlRpcSerializer();
      ser.SerializeRequest(stm, req);
      stm.Position = 0;
      TextReader tr = new StreamReader(stm);
      string reqstr = tr.ReadToEnd();

      Assertion.AssertEquals(@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>member1</name>
            <value>
              <i4>0</i4>
            </value>
          </member>
          <member>
            <name>member2</name>
            <value>
              <i4>0</i4>
            </value>
          </member>
          <member>
            <name>member-3</name>
            <value>
              <i4>0</i4>
            </value>
          </member>
          <member>
            <name>member-4</name>
            <value>
              <i4>0</i4>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
    }
         
         
    //---------------------- XmlRpcInt -------------------------------------// 
    public void testXmlRpcInt()
    {
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testXmlRpcInt", 
        new XmlRpcInt(12345), 
        Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;
      object obj = Utils.Parse(xdoc, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(12345, obj);
    }
  
    //---------------------- XmlRpcBoolean --------------------------------// 
    public void testXmlRpcBoolean()
    {
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testXmlRpcBoolean", 
        new XmlRpcBoolean(true), 
        Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;
      object obj = Utils.Parse(xdoc, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(true, obj);
    }

    //---------------------- XmlRpcDouble ----------------------------------// 
    public void testXmlRpcDouble()
    {
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testXmlRpcDouble", 
        new XmlRpcDouble(543.21), 
        Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;
      object obj = Utils.Parse(xdoc, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(543.21, obj);
    }

    public void testXmlRpcDouble_ForeignCulture()
    {
      CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
      XmlDocument xdoc;
      try
      {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-BE");
        XmlRpcDouble xsd = new XmlRpcDouble(543.21);
        //Console.WriteLine(xsd.ToString());
        xdoc = Utils.Serialize(
          "SerializeTest.testXmlRpcDouble_ForeignCulture", 
          new XmlRpcDouble(543.21), 
          Encoding.UTF8, MappingAction.Ignore);
      }
      catch(Exception)
      {
        throw;
      }
      finally
      {
        Thread.CurrentThread.CurrentCulture = currentCulture;
      }
      Type parsedType, parsedArrayType;
      object obj = Utils.Parse(xdoc, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(543.21, obj);
    }

    //---------------------- XmlRpcDateTime ------------------------------// 
    public void testXmlRpcDateTime()
    {
      XmlDocument xdoc = Utils.Serialize("SerializeTest.testXmlRpcDateTime", 
        new DateTime(2002, 7, 6, 11, 25, 37), 
        Encoding.UTF8, MappingAction.Ignore);
      Type parsedType, parsedArrayType;
      object obj = Utils.Parse(xdoc, null, MappingAction.Error, 
        out parsedType, out parsedArrayType);
      Assertion.AssertEquals(new DateTime(2002, 7, 6, 11, 25, 37), obj);
    }

    //---------------------- invalid type --------------------------------// 


    //---------------------- formatting ----------------------------------// 
    public void testDefaultFormatting()
    {
      Stream stm = new MemoryStream();
      XmlRpcRequest req = new XmlRpcRequest();
      req.args = new Object[] { 1234567 };
      req.method = "Foo";
      XmlRpcSerializer ser = new XmlRpcSerializer();
      ser.SerializeRequest(stm, req);
      stm.Position = 0;
      TextReader tr = new StreamReader(stm);
      string reqstr = tr.ReadToEnd();

      Assertion.AssertEquals(
@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
      <value>
        <i4>1234567</i4>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
    }

    public void testIncreasedIndentation()
    {
      Stream stm = new MemoryStream();
      XmlRpcRequest req = new XmlRpcRequest();
      req.args = new Object[] { 1234567 };
      req.method = "Foo";
      XmlRpcSerializer ser = new XmlRpcSerializer();
      ser.Indentation = 4;
      ser.SerializeRequest(stm, req);
      stm.Position = 0;
      TextReader tr = new StreamReader(stm);
      string reqstr = tr.ReadToEnd();

      Assertion.AssertEquals(
        @"<?xml version=""1.0""?>
<methodCall>
    <methodName>Foo</methodName>
    <params>
        <param>
            <value>
                <i4>1234567</i4>
            </value>
        </param>
    </params>
</methodCall>", reqstr);
    }

    public void testNoIndentation()
    {
      Stream stm = new MemoryStream();
      XmlRpcRequest req = new XmlRpcRequest();
      req.args = new Object[] { 1234567 };
      req.method = "Foo";
      XmlRpcSerializer ser = new XmlRpcSerializer();
      ser.UseIndentation = false;
      ser.SerializeRequest(stm, req);
      stm.Position = 0;
      TextReader tr = new StreamReader(stm);
      string reqstr = tr.ReadToEnd();

      Assertion.AssertEquals(
        "<?xml version=\"1.0\"?><methodCall><methodName>Foo</methodName>"+
        "<params><param><value><i4>1234567</i4></value></param></params>"+
        "</methodCall>", reqstr);
    }
  }

}
