using System;
using System.Collections.Generic;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class SuperBundleReply : XCMPReplyPacket
{
  private List<XCMPReplyPacket> messages = new List<XCMPReplyPacket>();

  public SuperBundleReply(byte[] data) : base(data)
  {
    if (ErrorCode == XCMPErrorCode.Success) {
      var msgCount = data[3];
      int offset = 4;
      for (int i = 0; i < msgCount; i++) {
        ushort msgLen = (ushort)((data[offset] << 8) | data[offset + 1]);
        var msgData = new byte[msgLen];
        Array.Copy(data, offset + 2, msgData, 0, msgLen);
        messages.Add(XCMPPacket.Decode(msgData) as XCMPReplyPacket);
        offset += msgLen + 2;
      }
    } else if (ErrorCode == XCMPErrorCode.OpcodeNotSupported && data.Length == 3)
    {
      throw new Exception("Super Bundle not supported");
    }
  }

  public XCMPReplyPacket this[int index] => messages[index];

  public int ChildCount => messages.Count;
}
