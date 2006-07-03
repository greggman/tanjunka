using System;
using System.IO;
using System.Reflection;
using  NUnit.Framework;
using CookComputing.XmlRpc;

namespace ntest
{       
  interface ITest
  {
    [XmlRpcMethod]
    string Method1(int x);
  }


  [TestFixture]
  public class ProxyGenTest
  {
    public void testMethod1()
    {
      ITest proxy = (ITest)XmlRpcProxyGen.Create(typeof(ITest));
      XmlRpcClientProtocol cp = (XmlRpcClientProtocol)proxy;
    }
  }
}