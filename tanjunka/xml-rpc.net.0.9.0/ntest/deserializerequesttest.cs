using System;
using System.IO;
using System.Reflection;
using  NUnit.Framework;
using CookComputing.XmlRpc;

namespace ntest
{       
  [TestFixture]
  public class DeserializeRequestTest
  {
    public void testStringElement()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestString</methodName> 
  <params>
    <param>
      <value><string>test string</string></value>
    </param>
  </params>
</methodCall>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
  
      Assertion.Assert("method is TestString", request.method == "TestString");
      Assertion.Assert("argument is string", request.args[0] is string);
      Assertion.Assert("argument is 'test string'", 
        (string)request.args[0] == "test string");

    }

    public void testStringNoStringElement()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestString</methodName> 
  <params>
    <param>
      <value>test string</value>
    </param>
  </params>
</methodCall>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
 
      Assertion.Assert("method is TestString", request.method == "TestString");
      Assertion.Assert("argument is string", request.args[0] is string);
      Assertion.Assert("argument is 'test string'", 
        (string)request.args[0] == "test string");
    }

    public void testStringEmptyValue1()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestString</methodName> 
  <params>
    <param>
      <value></value>
    </param>
  </params>
</methodCall>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
 
      Assertion.Assert("method is TestString", request.method == "TestString");
      Assertion.Assert("argument is string", request.args[0] is string);
      Assertion.Assert("argument is empty string", 
        (string)request.args[0] == "");
    }

    public void testStringEmptyValue2()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestString</methodName> 
  <params>
    <param>
      <value/>
    </param>
  </params>
</methodCall>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
 
      Assertion.Assert("method is TestString", request.method == "TestString");
      Assertion.Assert("argument is string", request.args[0] is string);
      Assertion.Assert("argument is empty string", 
        (string)request.args[0] == "");
    }

    public void testFlatXml()
    {
      string xml = @"<?xml version=""1.0"" ?><methodCall><methodName>TestString</methodName><params><param><value>test string</value></param></params></methodCall>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);

      Assertion.Assert("method is TestString", request.method == "TestString");
      Assertion.Assert("argument is string", request.args[0] is string);
      Assertion.Assert("argument is 'test string'", 
        (string)request.args[0] == "test string");
    }

    public void testNullRequestStream()
    {
      try
      {
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        Stream stm = null;
        XmlRpcRequest request = serializer.DeserializeRequest(stm, null);
        Assertion.Fail("Should throw ArgumentNullException");
      }
      catch (ArgumentNullException)
      {
      }
    }

    public void testEmptyRequestStream()
    {
      try
      {
        StringReader sr = new StringReader("");
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
        Assertion.Fail("Should throw XmlRpcIllFormedXmlException");
      }
      catch(XmlRpcIllFormedXmlException)
      {
      }
    }
    
    public void testInvalidXml()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall> </duffMmethodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
        Assertion.Fail("Should throw XmlRpcIllFormedXmlException");
      }
      catch(XmlRpcIllFormedXmlException)
      {
      }
    }

    // test handling of methodCall element
    public void testMissingMethodCall()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> <elem/>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
        Assertion.Fail("Should throw XmlRpcInvalidXmlRpcException");
      }
      catch(XmlRpcInvalidXmlRpcException)
      {
      }
    }

    // test handling of methodName element
    public void testMissingMethodName()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <params>
    <param>
      <value>test string</value>
    </param>
  </params>
</methodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
      }
      catch(Exception ex)
      {
        // TODO: should return InvalidXmlRpc
        string s = ex.Message;
      }
    }

    public void testEmptyMethodName()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName/> 
  <params>
    <param>
      <value>test string</value>
    </param>
  </params>
</methodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
      }
      catch(Exception ex)
      {

        // TODO: should return InvalidXmlRpc
        string s = ex.Message;
      }
    }

    public void testZeroLengthMethodName()
    {
      try 
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName></methodName> 
  <params>
    <param>
      <value>test string</value>
    </param>
  </params>
</methodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
      }
      catch(Exception ex)
      {
        // TODO: should return InvalidXmlRpc
        string s = ex.Message;
      }
    }

    public void testInvalidCharsMethodName()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName></methodName> 
  <params>
    <param>
      <value>test string</value>
    </param>
  </params>
</methodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
      }
      catch(Exception ex)
      {
        // TODO: should return InvalidXmlRpc
        string s = ex.Message;
      }
    }

    // test handling of params element
    public void testMissingParams()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestString</methodName> 
</methodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
      }
      catch(Exception ex)
      {
        string s = ex.Message;
      }
    }


    [XmlRpcMethod]
    public string MethodNoArgs()
    {
      return "";
    }

    // test handling of params element
    public void testNoParam1()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>MethodNoArgs</methodName> 
  <params/>
</methodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, this.GetType());
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex);
      }
    }

    // test handling of param element
    public void testNoParam2()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>MethodNoArgs</methodName> 
  <params>
  </params>
</methodCall>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcRequest request = serializer.DeserializeRequest(sr, this.GetType());
      //Console.WriteLine("");
    }

    public void testEmptyParam1()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestString</methodName> 
  <params>
    <param/>
  </params>
</methodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
      }
      catch(Exception ex)
      {
        string s = ex.Message;
      }
    }

    public void testEmptyParam2()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestString</methodName> 
  <params>
    <param>
    </param>
  </params>
</methodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
      }
      catch(Exception ex)
      {
        string s = ex.Message;
      }
    }

    // test handling integer values
    public void testInteger()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><int>666</int></value>
    </param>
  </params>
</methodCall>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);

      Assertion.Assert("method is TestInt", request.method == "TestInt");
      Assertion.Assert("argument is int", request.args[0] is int);
      Assertion.Assert("argument is 666", (int)request.args[0] == 666);
    }

    public void testI4Integer()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i4>666</i4></value>
    </param>
  </params>
</methodCall>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);

      Assertion.Assert("method is TestInt", request.method == "TestInt");
      Assertion.Assert("argument is int", request.args[0] is int);
      Assertion.Assert("argument is 666", (int)request.args[0] == 666);
    }

    public void testIntegerWithPlus()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i4>+666</i4></value>
    </param>
  </params>
</methodCall>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);

      Assertion.Assert("method is TestInt", request.method == "TestInt");
      Assertion.Assert("argument is int", request.args[0] is int);
      Assertion.Assert("argument is 666", (int)request.args[0] == 666);
    }

    public void testNegativeInteger()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i4>-666</i4></value>
    </param>
  </params>
</methodCall>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);

      Assertion.Assert("method is TestInt", request.method == "TestInt");
      Assertion.Assert("argument is int", request.args[0] is int);
      Assertion.Assert("argument is -666", (int)request.args[0] == -666);
    }

    public void testEmptyInteger()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i4></i4></value>
    </param>
  </params>
</methodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
      }
      catch(Exception ex)
      {
        string s = ex.Message;
      }
    }

    public void testInvalidInteger()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i4>12kiol</i4></value>
    </param>
  </params>
</methodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
        Assertion.Fail("Invalid integer should cause exception");
      }
      catch(Exception ex)
      {
        string s = ex.Message;
      }
    }

    public void testOverflowInteger()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i4>99999999999999999999</i4></value>
    </param>
  </params>
</methodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
      }
      catch(Exception ex)
      {
        string s = ex.Message;
      }
    }

    public void testZeroInteger()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i4>0</i4></value>
    </param>
  </params>
</methodCall>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);

      Assertion.Assert("method is TestInt", request.method == "TestInt");
      Assertion.Assert("argument is int", request.args[0] is int);
      Assertion.Assert("argument is 0", (int)request.args[0] == 0);
    }

    public void testNegativeOverflowInteger()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i4>-99999999999999999999</i4></value>
    </param>
  </params>
</methodCall>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
      }
      catch(Exception ex)
      {
        string s = ex.Message;
      }
    }

    public void testISO_8859_1()
    {
      using(Stream stm = new FileStream("../iso-8859-1_request.xml", 
              FileMode.Open, FileAccess.Read))
      {
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcRequest request = serializer.DeserializeRequest(stm, null);
        Assertion.Assert("argument is string", request.args[0] is string);
        Assertion.Assert("argument is 'hæ hvað segirðu þá'", 
          (string)request.args[0] == "hæ hvað segirðu þá");
      }
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
      string xml = @"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>member1</name>
            <value>
              <i4>1</i4>
            </value>
          </member>
          <member>
            <name>member2</name>
            <value>
              <i4>2</i4>
            </value>
          </member>
          <member>
            <name>member-3</name>
            <value>
              <i4>3</i4>
            </value>
          </member>
          <member>
            <name>member-4</name>
            <value>
              <i4>4</i4>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodCall>";

      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);

      Assertion.Assert("argument is XmlRpcStruct", 
        request.args[0] is XmlRpcStruct);
      XmlRpcStruct xrs = (XmlRpcStruct)request.args[0];
      Assertion.Assert("XmlRpcStruct has 4 members", xrs.Count == 4);
      Assertion.Assert("member1", xrs.ContainsKey("member1") 
        && (int)xrs["member1"] == 1);
      Assertion.Assert("member2", xrs.ContainsKey("member2") 
        && (int)xrs["member2"] == 2);
      Assertion.Assert("member-3", xrs.ContainsKey("member-3") 
        && (int)xrs["member-3"] == 3);
      Assertion.Assert("member-4", xrs.ContainsKey("member-4") 
        && (int)xrs["member-4"] == 4);
      




    }

    //    // test handling double values
    //
    //
    //    // test handling string values
    //
    //    // test handling dateTime values
    //
    //    // test handling base64 values
    //    public void testValidBase64()
    //    {
    //      string xml = @"<?xml version=""1.0"" ?> 
    //<methodCall>
    //  <methodName>TestStruct</methodName> 
    //  <params>
    //    <param>
    //    </param>
    //  </params>
    //</methodCall>";
    //      StringReader sr = new StringReader(xml);
    //      XmlRpcSerializer serializer = new XmlRpcSerializer();
    //      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
    //    }
    //
    //    public void testMultiLineBase64()
    //    {
    //      string xml = @"<?xml version=""1.0"" ?> 
    //<methodCall>
    //  <methodName>TestStruct</methodName> 
    //  <params>
    //    <param>
    //    </param>
    //  </params>
    //</methodCall>";
    //      StringReader sr = new StringReader(xml);
    //      XmlRpcSerializer serializer = new XmlRpcSerializer();
    //      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
    //    }
    //
    //    public void testInvalidBase64()
    //    {
    //      string xml = @"<?xml version=""1.0"" ?> 
    //<methodCall>
    //  <methodName>TestStruct</methodName> 
    //  <params>
    //    <param>
    //    </param>
    //  </params>
    //</methodCall>";
    //      StringReader sr = new StringReader(xml);
    //      XmlRpcSerializer serializer = new XmlRpcSerializer();
    //      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
    //    }
    //
    //    // test array handling
    //
    //
    //
    //    // tests of handling of structs
    //    public void testMissingMemberStruct()
    //    {
    //      string xml = @"<?xml version=""1.0"" ?> 
    //<methodCall>
    //  <methodName>TestStruct</methodName> 
    //  <params>
    //    <param>
    //    </param>
    //  </params>
    //</methodCall>";
    //      StringReader sr = new StringReader(xml);
    //      XmlRpcSerializer serializer = new XmlRpcSerializer();
    //      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
    //    }
    //
    //    public void testAdditonalMemberStruct()
    //    {
    //      string xml = @"<?xml version=""1.0"" ?> 
    //<methodCall>
    //  <methodName>TestStruct</methodName> 
    //  <params>
    //    <param>
    //    </param>
    //  </params>
    //</methodCall>";
    //      StringReader sr = new StringReader(xml);
    //      XmlRpcSerializer serializer = new XmlRpcSerializer();
    //      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
    //    }
    //
    //    public void testReversedMembersStruct()
    //    {
    //      string xml = @"<?xml version=""1.0"" ?> 
    //<methodCall>
    //  <methodName>TestStruct</methodName> 
    //  <params>
    //    <param>
    //    </param>
    //  </params>
    //</methodCall>";
    //      StringReader sr = new StringReader(xml);
    //      XmlRpcSerializer serializer = new XmlRpcSerializer();
    //      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
    //    }
    //    
    //    public void testWrongTypeMembersStruct()
    //    {
    //      string xml = @"<?xml version=""1.0"" ?> 
    //<methodCall>
    //  <methodName>TestStruct</methodName> 
    //  <params>
    //    <param>
    //    </param>
    //  </params>
    //</methodCall>";
    //      StringReader sr = new StringReader(xml);
    //      XmlRpcSerializer serializer = new XmlRpcSerializer();
    //      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
    //    }
    //
    //    public void testDuplicateMembersStruct()
    //    {
    //      string xml = @"<?xml version=""1.0"" ?> 
    //<methodCall>
    //  <methodName>TestStruct</methodName> 
    //  <params>
    //    <param>
    //    </param>
    //  </params>
    //</methodCall>";
    //      StringReader sr = new StringReader(xml);
    //      XmlRpcSerializer serializer = new XmlRpcSerializer();
    //      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
    //    }
    //
    //    public void testNonAsciiMemberNameStruct()
    //    {
    //      string xml = @"<?xml version=""1.0"" ?> 
    //<methodCall>
    //  <methodName>TestStruct</methodName> 
    //  <params>
    //    <param>
    //    </param>
    //  </params>
    //</methodCall>";
    //      StringReader sr = new StringReader(xml);
    //      XmlRpcSerializer serializer = new XmlRpcSerializer();
    //      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
    //    }
    //
    //    // test various invalid requests
    //    public void testIncorrectParamType()
    //    {
    //      string xml = @"<?xml version=""1.0"" ?> 
    //<methodCall>
    //  <methodName>TestStruct</methodName> 
    //  <params>
    //    <param>
    //    </param>
    //  </params>
    //</methodCall>";
    //      StringReader sr = new StringReader(xml);
    //      XmlRpcSerializer serializer = new XmlRpcSerializer();
    //      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
    //    }



  }
}



