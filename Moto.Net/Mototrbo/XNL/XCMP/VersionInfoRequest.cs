namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public enum VersionInfoType
    {
        HostSoftwareVersion = 0,
        BbfBundleVersion = 2,
        DspSoftwareVersion = 16,
        DspCompatibility = 17,
        DtpCompatibility = 19,
        MaceFlashVersion = 34,
        MaceHardwareVersion = 36,
        MaceHardwareType = 37,
        FlashBootAppVersion = 48,
        RamdownloaderVersion = 50,
        L3BootloaderVersion = 53,
        TuneVersion = 64,
        SecurityVersion = 65,
        CodeplugVersion = 66,
        PSDTVersion = 80,
        ConfigurationVersion = 81,
        KernelVersion = 82,
        FlashSize = 109,
        OptionBoardName = 130,
        OptionBoardHardwareType = 132,
        OptionBoardMainAppVersion = 133,
        OptionBoardFlashImageType = 135,
        OptionBoardFlashImageVersion = 136,
        ConsoletteBoardHWType = 164,
        ConsoletteBoardHostVersion = 165,
        FPGAControllerAltVersion = 176,
        FPGAControllerActiveVersion = 178,
        FPGAWirelineAltVersion = 179,
        FPGAWirelineFactoryVersion = 180,
        FPGAWirelineActiveVersion = 181,
    }

    public class VersionInfoRequest : XCMPPacket
    {
        public VersionInfoRequest(VersionInfoType type) : base(XCMPOpCode.VersionInfoRequest)
        {
            this.data = new byte[1];
            this.data[0] = (byte)type;
        }

        public VersionInfoRequest(byte[] data) : base(data)
        {
        }
    }
}
