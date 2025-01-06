using System.Linq;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class UUIDReply : XCMPReplyPacket
{
    protected byte[] uuid;

    public UUIDReply(byte[] data) : base(data)
    {
        if (ErrorCode == XCMPErrorCode.Success)
        {
            this.uuid = data.Skip(3).ToArray();
        }
    }

    public byte[] UUID
    {
        get
        {
            return uuid;
        }
    }
}
