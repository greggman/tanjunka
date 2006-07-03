using System;
using CookComputing.XmlRpc;

[XmlRpcUrl("http://www.cookcomputing.com/xmlrpcsamples/RPC2.ashx")]
class Betty : XmlRpcClientProtocol
{
  [XmlRpcMethod("examples.getStateName")]
  public string GetStateName(
    int stateNumber) 
  {
    return (string)Invoke("GetStateName", new object[] {stateNumber});
  }

  public IAsyncResult BeginGetStateName(
    int number, 
    AsyncCallback callback, 
    object asyncState)
  {
    return BeginInvoke("GetStateName", new object[] {number}, this, 
                        callback, asyncState);
  }

  public string EndGetStateName(
    IAsyncResult asr)
  { 
    string s = (string)EndInvoke(asr);
    return s;
  }
}
