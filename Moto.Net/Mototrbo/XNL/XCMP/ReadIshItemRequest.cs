using System;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class ReadIshItemRequest : XCMPPacket
{
  public ReadIshItemRequest(byte logicalPartition, ushort type, ushort id, ushort numBytes, ushort offset) : base(XCMPOpCode.ReadIshItemRequest)
  {
    data = new byte[9];
    data[0] = logicalPartition;
    data[1] = (byte)(type >> 8);
    data[2] = (byte)type;
    data[3] = (byte)(id >> 8);
    data[4] = (byte)id;
    data[5] = (byte)(numBytes >> 8);
    data[6] = (byte)numBytes;
    data[7] = (byte)(offset >> 8);
    data[8] = (byte)offset;
  }
}
