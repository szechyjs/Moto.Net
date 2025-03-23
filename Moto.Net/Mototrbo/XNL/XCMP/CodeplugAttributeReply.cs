using System;
using System.Linq;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class CodeplugAttributeReply : XCMPReplyPacket
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CodeplugAttributeOperation AttributeOperation { get; private set; }
        public CodeplugAttributeType AttributeType { get; private set; }
        public byte[] AttributeData { get; private set; }

        public CodeplugAttributeReply(byte[] data) : base(data)
        {
            if (data.Length < 5)
            {
                log.ErrorFormat("Failed to parse reply {0}", BitConverter.ToString(data));
                return;
            }
            AttributeOperation = (CodeplugAttributeOperation)data[3];
            AttributeType = (CodeplugAttributeType)data[4];

            var length = data[5];
            AttributeData = new byte[length];
            Array.Copy(data, 6, AttributeData, 0, length);
        }

        public byte SecurityCapability
        {
            get
            {
                if (AttributeType == CodeplugAttributeType.CertificateSupportedID && AttributeData != null && AttributeData.Length == 1)
                {
                    return AttributeData[0];
                }
                throw new InvalidOperationException();
            }
        }
    }
}
