using Moto.Net.Mototrbo;
using Moto.Net.Mototrbo.XNL;
using System;
using System.Net;

namespace Moto.Net
{
    public class LocalRadio : Radio
    {
        protected internal IPEndPoint ep;
        protected internal TCPClient client;

        public LocalRadio(RadioSystem sys, IPAddress ip)
        {
            this.sys = sys;
            this.ep = new IPEndPoint(ip, 8002);
            this.client = new TCPClient(ep);
        }

        public override bool InitXNL()
        {
            this.client.GotXNLXCMPPacket += new PacketHandler(HandleXNLPacket);
            this.xnlClient = new XNLClient(this, sys.ID);
            if (xnlClient.InitSuccess == false)
            {
                return false;
            }
            SetRadioID();
            return true;
        }

        private void SetRadioID()
        {
            xcmpClient = new Mototrbo.XNL.XCMP.XCMPClient(xnlClient);
            Mototrbo.XNL.XCMP.RadioStatusReply reply = xcmpClient.GetRadioStatus(Mototrbo.XNL.XCMP.XCMPStatus.RadioID);
            this.id = new RadioID(reply.Data);
        }

        public override void SendPacket(Packet pkt)
        {
            if (pkt.PacketType == PacketType.XnlXCMPPacket)
            {
                this.client.Send(((XNLXCMPPacket)pkt).XNLData);
                return;
            }
            throw new NotImplementedException("SendPacket doesn't support packet type " + pkt.PacketType);
        }

        private void HandleXNLPacket(object sender, PacketEventArgs e)
        {
            //Is the XNL Packet from the correct radio?
            if (this.ep.Equals(e.EP))
            {
                FireXNLPacket(e.Packet, e.EP);
            }
        }

        public void SecureUpgrade()
        {
            var ipPort = QuerySecureIPandPort();
            ep = new IPEndPoint(ipPort.RadioIP, ipPort.RadioPort);
            client.SecureUpgrade(ep);
            xnlClient.ReInit();
            SetRadioID();
        }
        protected override void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                client.Dispose();
            }

            isDisposed = true;
        }
    }
}
