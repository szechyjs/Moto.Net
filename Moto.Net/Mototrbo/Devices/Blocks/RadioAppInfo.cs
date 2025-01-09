namespace Moto.Net.Mototrbo.Devices.Blocks;

public class RadioAppInfo
{
  public static readonly Partition Partition = Partition.Application;
  public static readonly ushort ID = 71;

  private byte[] data;
  private ProductFamily family;

  public RadioAppInfo(ProductFamily family, byte[] data)
  {
    this.data = data;
    this.family = family;
  }

  public byte RI_LPGDAY => data[2];
  public byte RI_LPGMONTH => data[1];
  public byte RI_LPGYEAR => data[0];
  public byte RI_LPGHOUR => data[3];
  public byte RI_LPGMINUTES => data[4];
}
