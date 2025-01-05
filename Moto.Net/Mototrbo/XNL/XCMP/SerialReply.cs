using System;
using System.Linq;
using System.Text;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class SerialReply : XCMPReplyPacket
{
  protected String serial;

  public SerialReply(byte[] data) : base(data)
  {
    this.serial = ASCIIEncoding.ASCII.GetString(data.Skip(3).ToArray());
  }

  public String Serial
  {
    get
    {
      return this.serial;
    }
  }

  public override string ToString()
  {
    return base.ToString() + ": " + string.Join(",", this.data);
  }
}
