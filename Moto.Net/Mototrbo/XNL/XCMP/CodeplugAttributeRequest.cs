namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class CodeplugAttributeRequest : XCMPPacket
    {
        protected CodeplugAttributeOperation operationType;
        protected CodeplugAttributeType attrType;

        public CodeplugAttributeRequest(CodeplugAttributeOperation op, CodeplugAttributeType type) : base(XCMPOpCode.CodeplugAttrRequest)
        {
            operationType = op;
            attrType = type;
            this.data = new byte[3];
            this.data[0] = (byte)operationType;
            this.data[1] = (byte)attrType;
            this.data[2] = 0; // data length
            // TODO: add write support
        }

        public CodeplugAttributeRequest(byte[] data) : base(data)
        {
            this.operationType = (CodeplugAttributeOperation)data[3];
            this.attrType = (CodeplugAttributeType)data[4]; 
        }
    }
}
