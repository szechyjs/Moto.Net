using System;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class SerialRequest : XCMPPacket
{
  public SerialRequest() : base(XCMPOpCode.SerialRequest)
  {
    this.data = new byte[1];
    this.data[0] = 0x00; // Read, 0x01 = Write
  }
}
