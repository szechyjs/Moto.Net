namespace Moto.Net.Mototrbo.XNL.XCMP;

public class RadioModelRequest : XCMPPacket
{
  // This request requires unlocking the radio?
  public RadioModelRequest() : base(XCMPOpCode.RadioModelRequest)
  {
    this.data = new byte[1];
    this.data[0] = 0x00; // Read, 0x01 = Write
  }
}
