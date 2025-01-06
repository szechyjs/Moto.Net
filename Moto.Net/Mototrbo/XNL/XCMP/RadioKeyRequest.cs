using System;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class RadioKeyRequest : XCMPPacket
{
  public RadioKeyRequest() : base(XCMPOpCode.RadioKeyRequest) {}
}
