using Moto.Net.Mototrbo.Devices.Blocks;
using Moto.Net.Mototrbo.XNL.XCMP;
using Moto.Net.Util;

namespace Moto.Net.Mototrbo.Devices;

public class Paradise : Device
{
  private ProductFamily family;

  public Paradise(Radio radio, ProductFamily family) : base(radio)
  {
    this.family = family;
  }

  public override byte[] ValidationData()
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
