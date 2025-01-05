using System;
using System.Linq;
using System.Text;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class RadioModelReply : XCMPReplyPacket
{
  protected String model;

  public RadioModelReply(byte[] data) : base(data)
  {
    this.model = ASCIIEncoding.ASCII.GetString(data.Skip(3).ToArray());
  }

  public String Model
  {
    get
    {
      return this.model;
    }
  }

  public override string ToString()
  {
    return base.ToString() + ": " + string.Join(",", this.data);
  }
}
