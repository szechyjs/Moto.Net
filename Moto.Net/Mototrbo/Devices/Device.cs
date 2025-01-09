using System;

namespace Moto.Net.Mototrbo.Devices;

public class Device
{
  protected Radio radio;

  public Device(Radio radio)
  {
    this.radio = radio;
  }

  public static Device FromRadio(Radio radio)
  {
    var uuid = radio.UUID;
    if (uuid == null)
      return null;

    switch (BitConverter.ToString(uuid).Replace("-", ""))
    {
      case "0C78E1B906C54D3A8F264CE5C4F0B9DF": // Belize
      case "9F6C2442C375421981A8987115FC9ADE": // Malta
      case "0571AFE244664F999A96B020E82DC69C": // Andorra
      case "C52A3D4953FE469D8E11F05B143E8C56": // TahitiPlus
      case "106F58B631044D63B41F0C0D7720758D": // Reunion
      case "EBCCE9BF33B14896B5C2E7E3AA19AF0F": // TongaPlus
        return new Paradise(radio);
      case "0C0D6EE58204FBDEBB8860C631AB465A": // Timor
      case "1EC82E1A4AE2B4F1A8AC27E8039CB7E4": // Tonga
      case "C4FC39D8DEF24B779D1CB719AF26A269": // Tahiti
        // Paradise Light
      case "A36E7094861543C78796A6CDD04290B4": // Mackenzie
        // Paradise Prime
      case "AF5DAB63F4FC4926BB9000A6F18AF3DC":
        // Paradise Repeater
      case "D105ADD323864E539B513A65076458D3":
        // Matrix
      case "06CE7B7163C0456A845A6E13421F0AE4":
        // Phoenix
      default:
        return null;
    }
  }

  public virtual byte[] ValidationData() {
    throw new NotImplementedException();
  }
}
