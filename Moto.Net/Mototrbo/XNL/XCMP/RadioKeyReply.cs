using System.Linq;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class RadioKeyReply : XCMPReplyPacket
{
  protected byte[] key;
  protected XCMPErrorCode status;

  public RadioKeyReply(byte[] data) : base(data)
  {
    if (ErrorCode == XCMPErrorCode.Success)
    {
      this.key = data.Skip(3).ToArray();
    }
  }

  public byte[] Key
  {
    get
    {
      return key;
    }
  }
}
