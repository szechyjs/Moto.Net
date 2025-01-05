using System;
using System.Linq;
using System.Text;

namespace Moto.Net.Mototrbo.XNL.XCMP
{
    public class VersionInfoReply : XCMPReplyPacket
    {
        protected String version;

        public VersionInfoReply(byte[] data) : base(data)
        {
            this.version = ASCIIEncoding.ASCII.GetString(data.Skip(3).ToArray());
        }

        public String Version
        {
            get
            {
                return this.version;
            }
        }

        public override string ToString()
        {
            return base.ToString() + ": " + string.Join(",", this.data);
        }
    }
}
