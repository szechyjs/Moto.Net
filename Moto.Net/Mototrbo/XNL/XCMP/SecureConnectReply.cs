using System;
using System.Net;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class SecureConnectReply : XCMPReplyPacket
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FunctionOperation Operation { get; private set; }
        protected byte[] tlsOrIpData;

        public SecureConnectReply(byte[] data) : base(data)
        {
            if (data.Length < 3)
            {
                log.ErrorFormat("Failed to parse reply {0}", BitConverter.ToString(data));
                return;
            }

            Operation = (FunctionOperation)data[3];
            var length = data[4];
            tlsOrIpData = new byte[length];
            Array.Copy(data, 5, tlsOrIpData, 0, length);
        }

        public IPAddress RadioIP
        {
            get
            {
                if (Operation != FunctionOperation.ReadIpAndPort || tlsOrIpData == null || tlsOrIpData.Length != 6)
                    throw new InvalidOperationException();
                byte[] ip = new byte[4];
                Array.Copy(tlsOrIpData, ip, 4);
                return new IPAddress(ip);
            }
        }

        public ushort RadioPort
        {
            get
            {
                if (Operation != FunctionOperation.ReadIpAndPort || tlsOrIpData == null || tlsOrIpData.Length != 6)
                    throw new InvalidOperationException();
                byte[] portData = new byte[2];
                Array.Copy(tlsOrIpData, 4, portData, 0, 2);
                return (ushort)((portData[0] << 8) | portData[1]);
            }
        }
    }
}
