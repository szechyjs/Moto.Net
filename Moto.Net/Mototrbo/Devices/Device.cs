using System;
using Moto.Net.Mototrbo.Devices.Blocks;
using Moto.Net.Mototrbo.XNL.XCMP;
using Moto.Net.Util;

namespace Moto.Net.Mototrbo.Devices;

public class Device
{
  protected Radio radio;
  protected ProductFamily family;

  public Device(Radio radio)
  {
    this.radio = radio;
  }

  public Device(Radio radio, ProductFamily family)
  {
    this.radio = radio;
    this.family = family;
  }


  public static Device FromRadio(Radio radio)
  {
    var uuid = radio.UUID;
    if (uuid == null)
      return null;

    switch (BitConverter.ToString(uuid).Replace("-", ""))
    {
      // Paradise
      case "0C78E1B906C54D3A8F264CE5C4F0B9DF": // Belize
      case "9F6C2442C375421981A8987115FC9ADE": // Malta
      case "0571AFE244664F999A96B020E82DC69C": // Andorra
      case "C52A3D4953FE469D8E11F05B143E8C56": // TahitiPlus
      case "106F58B631044D63B41F0C0D7720758D": // Reunion
      case "EBCCE9BF33B14896B5C2E7E3AA19AF0F": // TongaPlus
        return new Device(radio, ProductFamily.ParadiseRadio);
      // Paradise Light
      case "0C0D6EE58204FBDEBB8860C631AB465A": // Timor
      case "1EC82E1A4AE2B4F1A8AC27E8039CB7E4": // Tonga
      case "C4FC39D8DEF24B779D1CB719AF26A269": // Tahiti
        return new Device(radio, ProductFamily.ParadiseLightRadio);
      // Paradise Prime
      case "A36E7094861543C78796A6CDD04290B4": // Mackenzie
        return new Device(radio, ProductFamily.ParadiseRadio);
      // Paradise Repeater
      case "AF5DAB63F4FC4926BB9000A6F18AF3DC":
        return new Device(radio, ProductFamily.ParadiseRepeaterRadio);
      case "D105ADD323864E539B513A65076458D3": // Matrix
        return new Device(radio, ProductFamily.MatrixRadio);
      case "06CE7B7163C0456A845A6E13421F0AE4": // Phoenix
        return new Device(radio, ProductFamily.PhoenixRadio);
      default:
        return null;
    }
  }

  public byte[] ValidationData()
  {
    var req = new SuperBundleRequest(true);
    req.AddMessage(new ReadIshItemRequest((byte)RadioSecInfo.Partition, RadioSecInfo.ID, 0, 0, 0));
    req.AddMessage(new ReadIshItemRequest((byte)FeatDescr.Partition, FeatDescr.ID, 0, 0, 0));
    req.AddMessage(new ReadIshItemRequest((byte)RadioAppInfo.Partition, RadioAppInfo.ID, 0, 0, 0));
    req.AddMessage(new ReadIshItemRequest((byte)NetSet.Partition, NetSet.ID, 0, 0, 0));
    var reply = radio.SendXCMP<SuperBundleReply>(req);

    if (reply.ErrorCode != XCMPErrorCode.Success)
      return null;

    req = new SuperBundleRequest(true);
    for (int i = 0; i < reply.ChildCount; i++)
    {
      var child = (ReadIshItemReply)reply[i];
      req.AddMessage(new ReadIshItemRequest(child.LogicalPartition, child.Type, child.ID, child.ItemSize, 0));
    }
    reply = radio.SendXCMP<SuperBundleReply>(req);

    if (reply.ErrorCode != XCMPErrorCode.Success)
      return null;

    var radioSecInfo = new RadioSecInfo(family, ((ReadIshItemReply)reply[0]).IshData);
    var featDesc = new FeatDescr(family, ((ReadIshItemReply)reply[1]).IshData);
    var radioAppInfo = new RadioAppInfo(family, ((ReadIshItemReply)reply[2]).IshData);
    var netSet = new NetSet(family, ((ReadIshItemReply)reply[3]).IshData);

    var validationData = Bytes.ConcatArrays(
      radioSecInfo.RS_SERIALNUM,
      featDesc.FD_FLASHID,
      radioSecInfo.RS_PROCID,
      radioSecInfo.RS_MODELNUM,
      [radioSecInfo.RS_ORGPROGDAY, radioSecInfo.RS_ORGPROGMONTH, radioSecInfo.RS_ORGPROGYEAR, radioSecInfo.RS_ORGPROGHOUR, radioSecInfo.RS_ORGPROGMINUTES],
      [radioAppInfo.RI_LPGDAY, radioAppInfo.RI_LPGMONTH, radioAppInfo.RI_LPGYEAR, radioAppInfo.RI_LPGHOUR, radioAppInfo.RI_LPGMINUTES],
      [radioSecInfo.RS_RGNID],
      [radioSecInfo.RS_ORGMAJAPPVERS, radioSecInfo.RS_ORGMINAPPVERS],
      [radioSecInfo.RS_ORGMAJSECVERS, radioSecInfo.RS_ORGMINSECVERS],
      netSet.NETSET_RDIPADDR,
      netSet.NETSET_RDSUBMASK
    );

    return validationData;
  }
}
