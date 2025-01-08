using System;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public enum DataType : byte
{
  None,
  Nand,
  FTL,
  File,
  Fxp,
  CompressFile,
}

public class XferDataRequest : XCMPPacket
{
  public XferDataRequest(ushort seqNum, DataType dataType, ushort length, byte[] payload) : base(XCMPOpCode.XferDataRequest)
  {
    data = new byte[5 + payload.Length];
    data[0] = (byte)dataType;
    data[1] = (byte)(seqNum >> 8);
    data[2] = (byte)seqNum;
    data[3] = (byte)(length >> 8);
    data[4] = (byte)length;
    if (payload != null)
      Array.Copy(payload, 0, data, 5, payload.Length);
  }

  public XferDataRequest(byte[] data) : base(data) {}
}
