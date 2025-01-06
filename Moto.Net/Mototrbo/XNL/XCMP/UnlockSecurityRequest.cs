using System;
using System.Configuration;

namespace Moto.Net.Mototrbo.XNL.XCMP;

public class UnlockSecurityRequest : XCMPPacket
{
    public UnlockSecurityRequest(byte[] radioKey) : base(XCMPOpCode.UnlockSecurityRequest) {
        this.data = EncryptRadioKey(radioKey);
    }

    private static byte[] EncryptRadioKey(byte[] radioKey) {

        int length = 32;
        byte[] result = new byte[length];
        uint seed = EncryptionSeed();
        for (int i = 0; i < length; ++i) {
            byte digit = radioKey[i];
            for (int j = 7; j > 0; --j) {
                int num = (int)(seed >> 9) & 1 ^ radioKey[length - i - 1] >> j & 1;
                seed = (uint)(((int)seed << 1) + num);
                digit ^= (byte)(num << j);
            }
            result[i] = digit;
        }
        return result;
    }

    private static uint EncryptionSeed() {
        string const1Str = ConfigurationManager.AppSettings.Get("UnlockSeed");
        return UInt32.Parse(const1Str);
    }
}
