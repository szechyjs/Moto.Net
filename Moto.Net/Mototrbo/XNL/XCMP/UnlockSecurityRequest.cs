using System;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class UnlockSecurityRequest : XCMPPacket
{
    public UnlockSecurityRequest(byte[] radioKey) : base(XCMPOpCode.UnlockSecurityRequest) {
        this.data = EncryptRadioKey(radioKey);
    }

    private static byte[] EncryptRadioKey(byte[] radioKey) {
        int length = 32;
        byte[] result = new byte[length];
        uint seed = 0; // TODO: load from config
        for (int i = 0; i < length; ++i) {
            byte digit = radioKey[i];
            for (int j = 7; j > 0; --j) {
                int num = digit >> 9 & 1 ^ radioKey[length - i - 1] >> j & 1;
                seed = (uint)(((int)seed << 1) + num);
                digit ^= (byte)(num << j);
            }
            result[i] = digit;
        }
        return result;
    }
}
