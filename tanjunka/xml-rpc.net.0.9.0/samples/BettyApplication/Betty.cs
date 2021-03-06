using System;
using CookComputing.XmlRpc;

struct StateStructRequest
{
  public int state1;  
  public int state2;
  public int state3;
}

[XmlRpcUrl("http://www.cookcomputing.com/xmlrpcsamples/RPC2.ashx")]
interface IStateName
{
  [XmlRpcMethod("examples.getStateName")]
  string GetStateName(int stateNumber); 

  [XmlRpcMethod("examples.getStateStruct")]
  string GetStateNames(StateStructRequest request);
}

