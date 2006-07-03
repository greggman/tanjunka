using System;
using System.IO;
using System.Reflection;
using  NUnit.Framework;
using CookComputing.XmlRpc;

namespace ntest
{       
  [TestFixture]
  public class DeserializeResponseTest 
  {

    // test return integer
    public void testI4NullType()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value><i4>12345</i4></value>
    </param>
  </params>
</methodResponse>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, null);

      Object o = response.retVal;
      Assertion.Assert("retval not null", o != null);
      Assertion.Assert("retval is int", o is int);
      Assertion.Assert("retval is 12345", (int)o == 12345);
    }

    public void testI4WithType()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value><i4>12345</i4></value>
    </param>
  </params>
</methodResponse>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, typeof(int));

      Object o = response.retVal;
      Assertion.Assert("retval not null", o != null);
      Assertion.Assert("retval is int", o is int);
      Assertion.Assert("retval is 12345", (int)o == 12345);
    }

    public void testIntegerNullType()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value><int>12345</int></value>
    </param>
  </params>
</methodResponse>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, null);

      Object o = response.retVal;
      Assertion.Assert("retval not null", o != null);
      Assertion.Assert("retval is int", o is int);
      Assertion.Assert("retval is 12345", (int)o == 12345);
    }

    public void testIntegerWithType()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value><int>12345</int></value>
    </param>
  </params>
</methodResponse>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, typeof(int));

      Object o = response.retVal;
      Assertion.Assert("retval not null", o != null);
      Assertion.Assert("retval is int", o is int);
      Assertion.Assert("retval is 12345", (int)o == 12345);
    }

    public void testIntegerIncorrectType()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value><int>12345</int></value>
    </param>
  </params>
</methodResponse>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcResponse response = serializer.DeserializeResponse(sr, typeof(string));
        Assertion.Fail("Should throw XmlRpcTypeMismatchException");
      }
      catch (XmlRpcTypeMismatchException)
      {
      }
    }
  
    // test return double

    // test return boolean

    // test return string
    public void testStringNullType()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value><string>test string</string></value>
    </param>
  </params>
</methodResponse>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, null);

      Object o = response.retVal;
      Assertion.Assert("retval not null", o != null);
      Assertion.Assert("retval is string", o is string);
      Assertion.Assert("retval is 'test string'", (string)o == "test string");
    }

    public void testString2NullType()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value>test string</value>
    </param>
  </params>
</methodResponse>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, null);

      Object o = response.retVal;
      Assertion.Assert("retval not null", o != null);
      Assertion.Assert("retval is string", o is string);
      Assertion.Assert("retval is 'test string'", (string)o == "test string");
    }

    public void testString1WithType()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value><string>test string</string></value>
    </param>
  </params>
</methodResponse>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, typeof(string));

      Object o = response.retVal;
      Assertion.Assert("retval not null", o != null);
      Assertion.Assert("retval is string", o is string);
      Assertion.Assert("retval is 'test string'", (string)o == "test string");
    }

    public void testString2WithType()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value>test string</value>
    </param>
  </params>
</methodResponse>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, typeof(string));

      Object o = response.retVal;
      Assertion.Assert("retval not null", o != null);
      Assertion.Assert("retval is string", o is string);
      Assertion.Assert("retval is 'test string'", (string)o == "test string");
    }

    public void testString1IncorrectType()
    {
      try
      {
        string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value>test string</value>
    </param>
  </params>
</methodResponse>";
        StringReader sr = new StringReader(xml);
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcResponse response = serializer.DeserializeResponse(sr, typeof(int));
        Assertion.Fail("Should throw XmlRpcTypeMismatchException");
      }
      catch(XmlRpcTypeMismatchException)
      {
      }
    }

    public void testStringEmptyValue()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value/>
    </param>
  </params>
</methodResponse>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, typeof(string));

      Object o = response.retVal;
      Assertion.Assert("retval not null", o != null);
      Assertion.Assert("retval is string", o is string);
      Assertion.Assert("retval is empty string", (string)o == "");
    }



    // test return dateTime


    // test return base64


    // test return array


    // test return struct


    public void testReturnStructAsObject()
    {
      string xml = @"<?xml version=""1.0"" encoding=""ISO-8859-1""?> 
<methodResponse>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>key3</name>
            <value>
              <string>this is a test</string>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodResponse>";

      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response 
        = serializer.DeserializeResponse(sr, typeof(object));
      
      Object o = response.retVal;
      string ret = (string)((XmlRpcStruct)o)["key3"];
    }


    public void testReturnStructAsXmlRpcStruct()
    {
      string xml = @"<?xml version=""1.0"" encoding=""ISO-8859-1""?> 
<methodResponse>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>key3</name>
            <value>
              <string>this is a test</string>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodResponse>";

      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response 
        = serializer.DeserializeResponse(sr, typeof(XmlRpcStruct));
      
      Object o = response.retVal;
      string ret = (string)((XmlRpcStruct)o)["key3"];
    }


   
    public void testArrayInStruct()
    {
      // reproduce problem reported by Alexander Agustsson
      string xml = @"<?xml version=""1.0"" encoding=""ISO-8859-1""?> 
<methodResponse>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>key3</name>
            <value>
              <array>
                <data>
                  <value>New Milk</value>
                  <value>Old Milk</value>
                </data>
              </array>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodResponse>";

      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, null);
      
      Object o = response.retVal;
      Assertion.Assert("retval is XmlRpcStruct", o is XmlRpcStruct);
      XmlRpcStruct xrs = (XmlRpcStruct)o;
      Assertion.Assert("retval contains one entry", xrs.Count == 1);
      object elem = xrs["key3"];
      Assertion.Assert("element has correct key", elem != null);
      Assertion.Assert("element is an array", elem is Array);
      object[] array = (object[])elem;
      Assertion.Assert("array has 2 members", array.Length == 2);
      Assertion.Assert("values of array members", 
        array[0] is string && (string)array[0] == "New Milk"
        && array[1] is string && (string)array[1] == "Old Milk");
    }


    public void testStringAndStructInArray()
    {
      // reproduce problem reported by Eric Brittain
      string xml = @"<?xml version=""1.0"" encoding=""ISO-8859-1""?> 
<methodResponse>
  <params>
    <param>
      <value>
        <array>
          <data>
            <value>
              <string>test string</string>
            </value>
            <value>
              <struct>
                <member>
                  <name>fred</name>
                  <value><string>test string 2</string></value>
                </member>
              </struct>
            </value>
          </data>
        </array>
      </value>
    </param>
  </params>
</methodResponse>";      

      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, null);
      Object o = response.retVal;

    }



    public struct InternalStruct
    {
      public string firstName;
      public string lastName;
    }

    public struct MyStruct
    {
      public string version;
      public InternalStruct record;
    }

    public void testReturnNestedStruct()
    {
      string xml = @"<?xml version=""1.0"" encoding=""ISO-8859-1""?> 
<methodResponse>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>version</name>
            <value><string>1.6</string></value>
          </member>
          <member>
            <name>record</name>
            <value>
              <struct>
                <member>
                  <name>firstName</name>
                  <value>Joe</value></member>
                <member>
                  <name>lastName</name>
                  <value>Test</value>
                </member>
              </struct>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodResponse>";

      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response 
        = serializer.DeserializeResponse(sr, typeof(MyStruct));
      
      Object o = response.retVal;
      Assertion.Assert("retval is MyStruct", o is MyStruct);
      MyStruct mystr = (MyStruct)o;
      Assertion.Assert("version is 1.6", mystr.version == "1.6");
      Assertion.Assert("firstname is Joe", 
        mystr.record.firstName == "Joe");
      Assertion.Assert("lastname is Test", 
        mystr.record.lastName == "Test");
    }

    public void testJoseProblem()
    {

      string xml = @"<?xml version='1.0'?> 
<methodResponse> 
<params> 
<param> 
<value><int>12</int></value> 
</param> 
</params> 

</methodResponse>"; 
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response 
        = serializer.DeserializeResponse(sr, typeof(int));
      
      Object o = response.retVal;
      Assertion.Assert("retval is int", o is int);
      int myint = (int)o;
      Assertion.Assert("int is 12", myint == 12);
    }

    struct BillStruct
    {
      public int x;
      public string s;
    }

    public void testMissingStructMember()
    {
      string xml = @"<?xml version='1.0'?> 
<methodResponse> 
  <params> 
    <param> 
      <value>
        <struct>
          <member>
            <name>x</name>
            <value>
              <i4>123</i4>
            </value>
          </member>
        </struct>
      </value> 
    </param> 
  </params> 
</methodResponse>"; 
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      try
      {
        XmlRpcResponse response 
          = serializer.DeserializeResponse(sr, typeof(BillStruct));
        Assertion.Fail("Should detect missing struct member");
      } 
      catch(AssertionException)
      {
        throw;
      }
      catch(Exception)
      {
      }    
    }

    public void testBillKeenanProblem()
    {
      string xml = @"<?xml version='1.0'?> 
<methodResponse> 
  <params> 
    <param> 
      <value>
        <struct>
          <member>
            <name>x</name>
            <value>
              <i4>123</i4>
            </value>
          </member>
          <member>
            <name>s</name>
            <value>
              <string>ABD~~DEF</string>
            </value>
          </member>
          <member>
            <name>unexpected</name>
            <value>
              <string>this is unexpected</string>
            </value>
          </member>
        </struct>
      </value> 
    </param> 
  </params> 
</methodResponse>"; 
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response 
        = serializer.DeserializeResponse(sr, typeof(BillStruct));
      
      Object o = response.retVal;
      Assertion.Assert("retval is BillStruct", o is BillStruct);
      BillStruct bs = (BillStruct)o;
      Assertion.Assert("struct members", 
        bs.x == 123 && bs.s == "ABD~~DEF");
    }

    public void testAdvogatoProblem()
    {
      string xml = @"<?xml version='1.0'?> 
<methodResponse>
<params>
<param>
<array>
<data>
<value>
<dateTime.iso8601>20020707T11:25:37</dateTime.iso8601>
</value>
<value>
<dateTime.iso8601>20020707T11:37:12</dateTime.iso8601>
</value>
</data>
</array>
</param>
</params>
</methodResponse>"; 
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      try 
      {
        XmlRpcResponse response 
          = serializer.DeserializeResponse(sr, null);
        Object o = response.retVal;
        Assertion.Fail("should have thrown XmlRpcInvalidXmlRpcException");
      }    
      catch(XmlRpcInvalidXmlRpcException)
      {
      }
    }

    public void testVoidReturnType()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value></value>
    </param>
  </params>
</methodResponse>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, typeof(void));
      Assertion.Assert("retval is null", response.retVal == null);
    }

    public void testEmptyValueReturn()
    {
      string xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value/>
    </param>
  </params>
</methodResponse>";
      StringReader sr = new StringReader(xml);
      XmlRpcSerializer serializer = new XmlRpcSerializer();
      XmlRpcResponse response = serializer.DeserializeResponse(sr, typeof(string));
      string s = (string)response.retVal;
      Assertion.Assert("retval is empty string", s == "");
    }


    public struct XmlRpcClassifyRequest
    {
      public int q_id;  
      public string docid;
      public string query;
      public string [] cattypes;
      public int topscores;
      public int timeout;
    }

    public struct user_info
    {
      public string username;
      public string password;
      public string hostname;
      public string ip;
    }

    public struct XmlRpcClassifyResult
    {
      public XmlRpcCatData [] categories;
      public string error_msg;
      public int error_code;
      public double exec_time;
      public int q_id;
      public string cattype;
    }

    public struct XmlRpcCatData
    {
      public int rank;
      public int cat_id;
      public string cat_title;
      public double composite_score;
      public string meta_info;
      public double [] component_scores;
    }

    public void testISO_8869_1()
    {
      using(Stream stm = new FileStream("../iso-8859-1_response.xml",
               FileMode.Open, FileAccess.Read))
      {
        XmlRpcSerializer serializer = new XmlRpcSerializer();
        XmlRpcResponse response 
          = serializer.DeserializeResponse(stm, typeof(String));
        String ret = (String)response.retVal;
        int nnn  = ret.Length;
        Assertion.Assert("retVal is 'hæ hvað segirðu þá'", 
          ret == "hæ hvað segirðu þá");
      }
    }



  }
}