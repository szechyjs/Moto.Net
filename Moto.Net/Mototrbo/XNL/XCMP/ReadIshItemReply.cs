using System;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class ReadIshItemReply : XCMPReplyPacket
{
  public byte LogicalPartition { get; private set; }
  public ushort Type { get; private set; }
  public ushort ID { get; private set; }
  public ushort NumberOfBytes { get; private set; }
  public ushort Offset { get; private set; }
  public ushort ItemSize { get; private set; }
  public byte[] IshData { get; private set; }

  public ReadIshItemReply(byte[] data) : base(data)
  {
    if (ErrorCode == XCMPErrorCode.Success)
    {
      LogicalPartition = data[3];
      Type = (ushort)((data[4] << 8) | data[5]);
      ID = (ushort)((data[6] << 8) | data[7]);
      NumberOfBytes = (ushort)((data[8] << 8) | data[9]);
      Offset = (ushort)((data[10] << 8) | data[11]);
      ItemSize = (ushort)((data[12] << 8) | data[13]);
      var ishData = new byte[NumberOfBytes];
      Array.Copy(data, 14, ishData, 0, NumberOfBytes);
      IshData = ishData;
    }
  }
}
