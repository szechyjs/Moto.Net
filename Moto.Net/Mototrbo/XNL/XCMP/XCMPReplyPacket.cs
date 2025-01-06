using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public enum XCMPErrorCode
    {
        Success = 0,
        Failure = 1,
        IncorrectMode = 2,
        OpcodeNotSupported = 3,
        InvalidParameter = 4,
        ReplyTooBig = 5,
        SecurityLocked = 6,
        BundledOpcodeNotSupported = 7,
        Busy = 16,
        BitLocked = 17,
        RadioIsLocked = 18,
        VoltageNotStable = 19,
        ProgramFailure = 20,
        TransferComplete = 22,
        RequestNotRxd = 23,
        SoftpotOperationNotSupported = 64,
        SoftpotTypeNotSupported = 65,
        SoftpotValueOutOfRange = 66,
        FlashWriteFailure = 128,
        ISHItemNotFound = 129,
        ISHOffsetOutOfRange = 130,
        ISHInsufficientPartitionSpace = 131,
        ISHPartitionDoesNotExist = 132,
        ISHPartitionReadOnly = 133,
        ISHReorgNeeded = 134,
        Undefined = 135,
    }

    public class XCMPReplyPacket : XCMPPacket
    {
        protected XCMPErrorCode errorCode;

        public XCMPReplyPacket(XCMPOpCode op) : base(op)
        {
            errorCode = XCMPErrorCode.Success;
        }

        public XCMPReplyPacket(byte[] data) : base(data)
        {
            errorCode = (XCMPErrorCode)data[2];
        }

        public XCMPErrorCode ErrorCode
        {
            get { return errorCode; }
        }
    }
}
