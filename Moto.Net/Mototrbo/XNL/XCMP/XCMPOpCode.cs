using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public enum XCMPOpCode
    {
        DeviceinitStatusBroadcast = 0xB400,
        RadioStatusRequest = 0x000E,
        RadioStatusReply = 0x800E,
        VersionInfoRequest = 0x000F,
        VersionInfoReply = 0x800F,
        RadioModelRequest = 0x0010,
        RadioModelReply = 0x8010,
        SerialRequest = 0x0011,
        SerialReply = 0x8011,
        UUIDRequest = 0x0012,
        UUIDReply = 0x8012,
        DateCodeRequest = 0x0019,
        DateCodeReply = 0x8019,
        TANAPARequest = 0x001F,
        TANAPAReply = 0x801F,
        LangPackInfoRequest = 0x002C,
        LangPackInfoReply = 0x802C,
        SuperBundleRequest = 0x002E,
        SuperBundleReply = 0x802E,
        CodeplugAttrRequest = 0x0037,
        CodeplugAttrReply = 0x8037,
        ReadISHItemRequest = 0x0100,
        ReadISHItemReply = 0x8100,
        ISHUnlockReportRequest = 0x0108,
        ISHUnlockReportReply = 0x8108,
        CloneReadRequest = 0x010A,
        CloneReadReply = 0x810A,
        RadioKeyRequest = 0x0300,
        RadioKeyReply = 0x8300,
        UnlockSecurityRequest = 0x0301,
        UnlockSecurityReply = 0x8301,
        ChannelSelectRequest = 0x040D,
        ChannelSelectReply = 0x840D,
        RRCtrlBroadcast = 0xB41C,
        AlarmStatusRequest = 0x042E,
        AlarmStatusReply = 0x842E,
        XferDataRequest = 0x0446,
        XferDataReply = 0x8446,
    }
    //Unknown packet: {OpCode: 0000B402, Data: 01-09-00-00-0C-01-05-00-01-00-04-00}

    public static class EnumExtension
    {
        public static void AddToArray(this XCMPOpCode opcode, byte[] array, int startOffset)
        {
            UInt16 intval = (UInt16)opcode;
            byte[] bytes = BitConverter.GetBytes(intval);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            Array.Copy(bytes, 0, array, startOffset, 2);
        }
    }
}
