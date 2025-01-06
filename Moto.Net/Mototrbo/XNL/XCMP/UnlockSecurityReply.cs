namespace Moto.Net.Mototrbo.XNL.XCMP;

public class UnlockSecurityReply : XCMPReplyPacket
{
    protected bool success;

    public UnlockSecurityReply(byte[] data) : base(data)
    {
        this.success = data[2] == 0x00;
    }

    public bool Success
    {
        get
        {
            return success;
        }
    }
}
