using System;
using System.Collections.Generic;
using System.Linq;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class SuperBundleRequest : XCMPPacket
{
  private bool continueFail;
  private List<XCMPPacket> messages = new List<XCMPPacket>();

  public SuperBundleRequest(bool continueFail) : base(XCMPOpCode.SuperBundleRequest)
  {
    this.continueFail = continueFail;
    buildData();
  }

  private void buildData()
  {
    data = new byte[MaxMessageSize];
    data[0] = (byte)messages.Count;
    data[1] = continueFail ? (byte)1 : (byte)0;
    int offset = 2;
    messages.ForEach((message) =>
    {
      ushort msgLen = message.Length;
      data[offset] = (byte)((msgLen >> 8) & 0xFF);
      data[offset + 1] = (byte)(msgLen & 0xFF);
      Array.Copy(message.Encode(), 0, data, offset + 2, message.Length);
      offset += message.Length + 2;
    });
  }

  public bool AddMessage(XCMPPacket message)
  {
    if (message == null)
      return false;

    messages.Add(message);
    buildData();
    return true;
  }

  public int MaxMessageSize
  {
    get
    {
      int size = 2; // super bundle header
      size += messages.Sum((message) => 2 + message.Length);
      return size;
    }
  }
}
