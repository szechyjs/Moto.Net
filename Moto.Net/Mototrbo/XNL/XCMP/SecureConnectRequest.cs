namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class SecureConnectRequest : XCMPPacket
    {
        protected FunctionOperation operation;

        public SecureConnectRequest(FunctionOperation operation) : base(XCMPOpCode.SecureConnectRequest)
        {
            this.operation = operation;
            this.data = new byte[2];
            this.data[0] = (byte)operation;
            this.data[1] = 0;
        }
    }
}
