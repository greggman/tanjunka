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
{
  [TestFixture]
  public class OptionalSerializeTest
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

    [XmlRpcMissingMapping(MappingAction.Error)]
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
      [XmlRpcMissingMapping(MappingAction.Error)]
      public int mi;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public string ms;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public bool mb;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public double md;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public DateTime mdt;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public byte[] mb64;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public int[] ma;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public XmlRpcInt xi;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public XmlRpcBoolean xb;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public XmlRpcDouble xd;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public XmlRpcDateTime xdt;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public XmlRpcStruct xstr;
    }

    [XmlRpcMissingMapping(MappingAction.Ignore)]
    struct Struct4
    {
      [XmlRpcMissingMapping(MappingAction.Error)]
      public int mi;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public string ms;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public bool mb;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public double md;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public DateTime mdt;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public byte[] mb64;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public int[] ma;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public XmlRpcInt xi;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public XmlRpcBoolean xb;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public XmlRpcDouble xd;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public XmlRpcDateTime xdt;
      [XmlRpcMissingMapping(MappingAction.Error)]
      public XmlRpcStruct xstr;
    }

    //----------------------------------------------------------------------// 
    public void testStruct1_AllMissing_Error()
    {
      try
      {
        XmlDocument xdoc = Utils.Serialize("testStruct1_AllMissingError", 
          new Struct1(), 
          Encoding.UTF8, MappingAction.Error);
      }
      catch(XmlRpcNullReferenceException)
      {
      }
      catch(Exception ex)
      {
        Assertion.Fail(String.Format("unexpected exception: {0}\n", 
          ex.Message));
      }
    }

    public void testStruct2_AllMissing_IgnoreStructError()
    {
      try
      {
        XmlDocument xdoc = Utils.Serialize(
          "testStruct2_AllMissing_IgnoreStructError", 
          new Struct1(), 
          Encoding.UTF8, MappingAction.Ignore);
      }
      catch(XmlRpcNullReferenceException)
      {
      }
      catch(Exception ex)
      {
        Assertion.Fail(String.Format("unexpected exception: {0}\n", 
          ex.Message));
      }
    }

    public void testStruct3_AllMissing_IgnoreMemberError()
    {
      try
      {
        XmlDocument xdoc = Utils.Serialize(
          "testStruct3_AllMissing_IgnoreMemberError", 
          new Struct1(), 
          Encoding.UTF8, MappingAction.Ignore);
      }
      catch(XmlRpcNullReferenceException)
      {
      }
      catch(Exception ex)
      {
        Assertion.Fail(String.Format("unexpected exception: {0}\n", 
          ex.Message));
      }
    }

    public void testStruct4_AllMissing_ErrorStructIgnoreMemberError()
    {
      try
      {
        XmlDocument xdoc = Utils.Serialize(
          "testStruct4_AllMissing_ErrorStructIgnoreMemberError", 
          new Struct1(), 
          Encoding.UTF8, MappingAction.Error);
      }
      catch(XmlRpcNullReferenceException)
      {
      }
      catch(Exception ex)
      {
        Assertion.Fail(String.Format("unexpected exception: {0}\n", 
          ex.Message));
      }
    }

    //----------------------------------------------------------------------// 
  }
}