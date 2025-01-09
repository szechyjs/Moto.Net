using System;
using System.Linq;

namespace Moto.Net.Mototrbo.Devices.Blocks;

public class NetSet
{
  public static readonly Partition Partition = Partition.Application;
  public static readonly ushort ID = 114;

  private byte[] data;
  private ProductFamily family;

  public NetSet(ProductFamily family, byte[] data)
  {
    this.data = data;
    this.family = family;
  }

  public byte[] NETSET_RDIPADDR => data.Skip(8).Take(4).Reverse().ToArray();
  public byte[] NETSET_RDSUBMASK => data.Skip(4).Take(4).Reverse().ToArray();
  public byte NETSET_CAINETID => data[0];
}
