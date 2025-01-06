using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moto.Net.Mototrbo.XNL
{
    public class DevConnectionRequestPacket : XNLPacket
    {
        protected Address connection;
        protected byte deviceType;
        protected byte authenticationLevel;
        protected byte[] key;

        public DevConnectionRequestPacket() : base(OpCode.DeviceConnectionRequest)
        {
        }

        public DevConnectionRequestPacket(Address dest, Address src, Address connectionAddress, byte deviceType, byte authenticationLevel, byte[] key, bool repeater) : base(OpCode.DeviceConnectionRequest)
        {
            this.dest = dest;
            this.src = src;
            this.connection = connectionAddress;
            this.deviceType = deviceType;
            this.authenticationLevel = authenticationLevel;
            if (authenticationLevel == 0)
            {
                this.key = Encrypter.EncryptSuper(key);
            }
            else if (repeater)
            {
                this.key = Encrypter.Encrypt(key);
            }
            else
            {
                this.key = Encrypter.EncryptControlStation(key);
            }
            this.data = new byte[4+this.key.Length];
            this.connection.AddToArray(this.data, 0);
            this.data[2] = this.deviceType;
            this.data[3] = this.authenticationLevel;
            Array.Copy(this.key, 0, this.data, 4, this.key.Length);
        }
    }
}
