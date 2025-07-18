using System.Linq;

namespace Moto.Net.Mototrbo.Devices.Blocks;

public class FeatDescr
{
  public static readonly Partition Partition = Partition.Security;
  public static readonly ushort ID = 4113;

  private byte[] data;
  private ProductFamily family;

  public FeatDescr(ProductFamily family, byte[] data)
  {
    this.data = data;
    this.family = family;
  }

  public byte[] FD_FLASHID {
    get
    {
      if (family == ProductFamily.ParadiseRadio ||
        family == ProductFamily.ParadiseLightRadio ||
        family == ProductFamily.DenaliRadio ||
        family == ProductFamily.PhoenixRadio)
        return data.Skip(28).Take(16).ToArray();
      else if (family == ProductFamily.ParadiseRepeaterRadio ||
        family == ProductFamily.MatrixRadio ||
        family == ProductFamily.MatrixRepeaterRadio)
        return data.Skip(28).Take(14).ToArray();
      else
        throw new System.Exception("Unsupported family for FD_FLASHID: " + family);
    }
  }
}
