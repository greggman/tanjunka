using System;
using System.IO;
using System.Xml;
using System.Reflection;
using NUnit.Framework;
using CookComputing.XmlRpc;


class XmlRpcStruct1 : XmlRpcStruct
{
  public int mi;
}

namespace ntest 
{

  [TestFixture]
  public class OptionalDeserializeTest
  {
    struct Struct1
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

    [XmlRpcMissingMapping(MappingAction.Ignore)]
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

    struct Struct3
    {
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public int mi;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public string ms;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public bool mb;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public double md;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public DateTime mdt;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public byte[] mb64;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public int[] ma;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public XmlRpcInt xi;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public XmlRpcBoolean xb;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public XmlRpcDouble xd;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public XmlRpcDateTime xdt;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public XmlRpcStruct xstr;
    }

    [XmlRpcMissingMapping(MappingAction.Error)]
      struct Struct4
    {
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public int mi;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public string ms;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public bool mb;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public double md;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public DateTime mdt;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public byte[] mb64;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public int[] ma;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public XmlRpcInt xi;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public XmlRpcBoolean xb;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public XmlRpcDouble xd;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public XmlRpcDateTime xdt;
      [XmlRpcMissingMapping(MappingAction.Ignore)]
      public XmlRpcStruct xstr;
    }

    public void testStruct1_AllMissing_Error()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?><value><struct></struct></value>";
      try
      {
        object obj = Utils.Parse(xml, typeof(Struct1), MappingAction.Error, 
          out parsedType, out parsedArrayType);
        Assertion.Fail("didn't detect missing members");
      }
      catch (XmlRpcTypeMismatchException)
      {
      }
      catch(Exception ex)
      {
        Assertion.Fail("unexpected exception: " + ex.Message);
      }
    }    
  
    public void testStruct1_AllMissing_Ignore()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?><value><struct></struct></value>";
      try
      {
        object obj = Utils.Parse(xml, typeof(Struct1), MappingAction.Ignore, 
          out parsedType, out parsedArrayType);
        Assertion.Assert("obj is Struct1", obj is Struct1);
        Assertion.AssertEquals("int member", 0, ((Struct1)obj).mi);
        Assertion.AssertEquals("string member", null, ((Struct1)obj).ms);
        Assertion.AssertEquals("boolean member", false, ((Struct1)obj).mb);
        Assertion.AssertEquals("double member", 0.0, ((Struct1)obj).md);
        Assertion.AssertEquals("dateTime member", new DateTime(), 
          ((Struct1)obj).mdt);
        Assertion.AssertEquals("base64 member", null, ((Struct1)obj).mb64); 
        Assertion.AssertEquals("array member", null, ((Struct1)obj).ma); 
        Assertion.AssertEquals("XmlRpcInt member", null, ((Struct1)obj).xi); 
        Assertion.AssertEquals("XmlRpcBoolean member", null, 
          ((Struct1)obj).xb); 
        Assertion.AssertEquals("XmlRpcDouble member", null, 
          ((Struct1)obj).xd); 
        Assertion.AssertEquals("XmlRpcDateTime member", null, 
          ((Struct1)obj).xdt);                
        Assertion.AssertEquals("XmlRpcStructTime member", null, 
          ((Struct1)obj).xstr);                
      }
      catch (XmlRpcTypeMismatchException mmex)
      {
        Assertion.Fail("unexpected XmlRpcTypeMismatchException: " + mmex.Message);
      }
      catch(Exception ex)
      {
        Assertion.Fail("unexpected exception: " + ex.Message);
      }
    }
  
    public void testStruct2_AllMissing_ErrorStructIgnore()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?><value><struct></struct></value>";
      try
      {
        object obj = Utils.Parse(xml, typeof(Struct2), MappingAction.Error, 
          out parsedType, out parsedArrayType);
        Assertion.Assert("obj is Struct2", obj is Struct2);
        Assertion.AssertEquals("int member", 0, ((Struct2)obj).mi);
        Assertion.AssertEquals("string member", null, ((Struct2)obj).ms);
        Assertion.AssertEquals("boolean member", false, ((Struct2)obj).mb);
        Assertion.AssertEquals("double member", 0.0, ((Struct2)obj).md);
        Assertion.AssertEquals("dateTime member", new DateTime(), 
          ((Struct2)obj).mdt);
        Assertion.AssertEquals("base64 member", null, ((Struct2)obj).mb64);        
        Assertion.AssertEquals("array member", null, ((Struct2)obj).ma); 
        Assertion.AssertEquals("XmlRpcInt member", null, ((Struct2)obj).xi); 
        Assertion.AssertEquals("XmlRpcBoolean member", null, 
          ((Struct2)obj).xb); 
        Assertion.AssertEquals("XmlRpcDouble member", null, 
          ((Struct2)obj).xd); 
        Assertion.AssertEquals("XmlRpcDateTime member", null, 
          ((Struct2)obj).xdt);                
        Assertion.AssertEquals("XmlRpcStructTime member", null, 
          ((Struct2)obj).xstr);                
      }
      catch (XmlRpcTypeMismatchException mmex)
      {
        Assertion.Fail("unexpected XmlRpcTypeMismatchException: " + mmex.Message);
      }
      catch(Exception ex)
      {
        Assertion.Fail("unexpected exception: " + ex.Message);
      }
    }

    public void testStruct3_AllMissing_ErrorMemberIgnore()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?><value><struct></struct></value>";
      try
      {
        object obj = Utils.Parse(xml, typeof(Struct3), MappingAction.Error, 
          out parsedType, out parsedArrayType);
        Assertion.Assert("obj is Struct3", obj is Struct3);
        Assertion.AssertEquals("int member", 0, ((Struct3)obj).mi);
        Assertion.AssertEquals("string member", null, ((Struct3)obj).ms);
        Assertion.AssertEquals("boolean member", false, ((Struct3)obj).mb);
        Assertion.AssertEquals("double member", 0.0, ((Struct3)obj).md);
        Assertion.AssertEquals("dateTime member", new DateTime(), 
          ((Struct3)obj).mdt);
        Assertion.AssertEquals("base64 member", null, ((Struct3)obj).mb64);        
        Assertion.AssertEquals("array member", null, ((Struct3)obj).ma); 
        Assertion.AssertEquals("XmlRpcInt member", null, ((Struct3)obj).xi); 
        Assertion.AssertEquals("XmlRpcBoolean member", null, 
          ((Struct3)obj).xb); 
        Assertion.AssertEquals("XmlRpcDouble member", null, 
          ((Struct3)obj).xd); 
        Assertion.AssertEquals("XmlRpcDateTime member", null, 
          ((Struct3)obj).xdt);                
        Assertion.AssertEquals("XmlRpcStructTime member", null, 
          ((Struct3)obj).xstr);                
      }
      catch (XmlRpcTypeMismatchException mmex)
      {
        Assertion.Fail("unexpected XmlRpcTypeMismatchException: " 
          + mmex.Message);
      }
      catch(Exception ex)
      {
        Assertion.Fail("unexpected exception: " + ex.Message);
      }
    }

    public void testStruct4_AllMissing_ErrorStructErrorMemberIgnore()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?><value><struct></struct></value>";
      try
      {
        object obj = Utils.Parse(xml, typeof(Struct4), MappingAction.Error, 
          out parsedType, out parsedArrayType);
        Assertion.Assert("obj is Struct4", obj is Struct4);
        Assertion.AssertEquals("int member", 0, ((Struct4)obj).mi);
        Assertion.AssertEquals("string member", null, ((Struct4)obj).ms);
        Assertion.AssertEquals("boolean member", false, ((Struct4)obj).mb);
        Assertion.AssertEquals("double member", 0.0, ((Struct4)obj).md);
        Assertion.AssertEquals("dateTime member", new DateTime(), 
          ((Struct4)obj).mdt);
        Assertion.AssertEquals("base64 member", null, ((Struct4)obj).mb64);        
        Assertion.AssertEquals("array member", null, ((Struct4)obj).ma); 
        Assertion.AssertEquals("XmlRpcInt member", null, ((Struct4)obj).xi); 
        Assertion.AssertEquals("XmlRpcBoolean member", null, 
          ((Struct4)obj).xb); 
        Assertion.AssertEquals("XmlRpcDouble member", null, 
          ((Struct4)obj).xd); 
        Assertion.AssertEquals("XmlRpcDateTime member", null, 
          ((Struct4)obj).xdt);                
        Assertion.AssertEquals("XmlRpcStructTime member", null, 
          ((Struct4)obj).xstr);                
      }
      catch (XmlRpcTypeMismatchException mmex)
      {
        Assertion.Fail("unexpected XmlRpcTypeMismatchException: " 
          + mmex.Message);
      }
      catch(Exception ex)
      {
        Assertion.Fail("unexpected exception: " + ex.Message);
      }
    }
      
   public void testStruct4_AllMissing_IgnoreStructErrorMemberIgnore()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?><value><struct></struct></value>";
      try
      {
        object obj = Utils.Parse(xml, typeof(Struct4), MappingAction.Ignore, 
          out parsedType, out parsedArrayType);
        Assertion.Assert("obj is Struct4", obj is Struct4);
        Assertion.AssertEquals("int member", 0, ((Struct4)obj).mi);
        Assertion.AssertEquals("string member", null, ((Struct4)obj).ms);
        Assertion.AssertEquals("boolean member", false, ((Struct4)obj).mb);
        Assertion.AssertEquals("double member", 0.0, ((Struct4)obj).md);
        Assertion.AssertEquals("dateTime member", new DateTime(), 
          ((Struct4)obj).mdt);
        Assertion.AssertEquals("base64 member", null, ((Struct4)obj).mb64);        
        Assertion.AssertEquals("array member", null, ((Struct4)obj).ma); 
        Assertion.AssertEquals("XmlRpcInt member", null, ((Struct4)obj).xi); 
        Assertion.AssertEquals("XmlRpcBoolean member", null, 
          ((Struct4)obj).xb); 
        Assertion.AssertEquals("XmlRpcDouble member", null, 
          ((Struct4)obj).xd); 
        Assertion.AssertEquals("XmlRpcDateTime member", null, 
          ((Struct4)obj).xdt);                
        Assertion.AssertEquals("XmlRpcStructTime member", null, 
          ((Struct4)obj).xstr);                
      }
      catch (XmlRpcTypeMismatchException mmex)
      {
        Assertion.Fail("unexpected XmlRpcTypeMismatchException: " 
          + mmex.Message);
      }
      catch(Exception ex)
      {
        Assertion.Fail("unexpected exception: " + ex.Message);
      }
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    struct StructOuter1
    {
      public StructInner1 mstruct;
    }

    struct StructInner1
    {
      public int mi;
    }

    public void testNoInnerStructOverrideIgnoreError()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
      <value>
        <struct>
          <member>
            <name>mstruct</name>
            <value><struct></struct></value>
          </member>
        </struct>
      </value>";
      try
      {
        object obj = Utils.Parse(xml, typeof(StructOuter1), MappingAction.Error, 
          out parsedType, out parsedArrayType);
        Assertion.Fail("didn't detect missing members");
      }
      catch (XmlRpcTypeMismatchException)
      {
      }
      catch(Exception ex)
      {
        Assertion.Fail("unexpected exception: " + ex.Message);
      }
    }

    [XmlRpcMissingMapping(MappingAction.Error)]
      struct StructOuter2
    {
      public StructInner2 mstruct;
    }

    struct StructInner2
    {
      public int mi;
    }

    public void testNoInnerStructOverrideErrorIgnore()
    {
      Type parsedType, parsedArrayType;
      string xml = @"<?xml version=""1.0"" ?>
      <value>
        <struct>
          <member>
            <name>mstruct</name>
            <value><struct></struct></value>
          </member>
        </struct>
      </value>";
      try
      {
        object obj = Utils.Parse(xml, typeof(StructOuter2), 
          MappingAction.Ignore, 
          out parsedType, out parsedArrayType);
      }
      catch (XmlRpcTypeMismatchException)
      {
        Assertion.Fail("falsely detected missing member in inner struct");
      }
      catch(Exception ex)
      {
        Assertion.Fail("unexpected exception: " + ex.Message);
      }
    }

    //------------------------ XmlRpcStruct Derived ------------------------// 

  }
}