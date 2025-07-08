using System.Linq;

namespace Moto.Net.Mototrbo.Devices.Blocks;

public class RadioSecInfo
{
  public static readonly Partition Partition = Partition.Security;
  public static readonly ushort ID = 4112;

  private byte[] data;
  private ProductFamily family;

  public RadioSecInfo(ProductFamily family, byte[] data)
  {
    this.data = data;
    this.family = family;
  }

  public byte[] RS_SERIALNUM => data.Take(10).ToArray();
  public byte[] RS_PROCID
  {
    get
    {
      if (family == ProductFamily.ParadiseRepeaterRadio)
        return data.Skip(96).Take(8).Reverse().ToArray();
      else
        return [data[99], data[98], data[97], data[96], data[103], data[102], data[101], data[100]];
    }
  }
  public byte[] RS_MODELNUM => data.Skip(12).Take(12).ToArray();
  public byte RS_ORGPROGDAY => data[32];
  public byte RS_ORGPROGMONTH => data[31];
  public byte RS_ORGPROGYEAR => data[30];
  public byte RS_ORGPROGHOUR => data[33];
  public byte RS_ORGPROGMINUTES => data[34];
  public byte RS_ORGMAJAPPVERS => data[38];
  public byte RS_ORGMINAPPVERS => data[39];
  public byte RS_ORGMAJSECVERS => data[36];
  public byte RS_ORGMINSECVERS => data[37];
  public byte RS_RGNID => data[78];
}
