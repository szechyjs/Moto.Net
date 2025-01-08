using System;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class XferDataReply: XCMPReplyPacket
{
  protected DataType dataType;
  protected ushort seqNum;
  protected ushort length;
  protected byte[] payload;

  public XferDataReply(byte[] data) : base(data)
  {
    dataType = (DataType)data[3];
    seqNum = (ushort)((data[4] << 8) | data[5]);
    length = (ushort)((data[6] << 8) | data[7]);
    if (payload == null)
      payload = new byte[length];

    if (data.Length - 8 < length)
      return;

    Array.Copy(data, 8, payload, 0, length);
  }

  public DataType DataType
  {
    get
    {
      return dataType;
    }
  }

  public ushort SeqNum
  {
    get
    {
      return seqNum;
    }
  }

  public ushort Length
  {
    get
    {
      return length;
    }
  }

  public byte[] Payload
  {
    get
    {
      return payload;
    }
  }
}
