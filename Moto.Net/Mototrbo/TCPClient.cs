using Moto.Net.Mototrbo.XNL;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NippyWard.OpenSSL.Keys;
using NippyWard.OpenSSL.SSL;
using NippyWard.OpenSSL.X509;
using System.IO;
using System.Configuration;
using System.Collections.Generic;

namespace Moto.Net.Mototrbo
{
    public class TCPClient : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected TcpClient rawClient;
        protected IPEndPoint ep;
        protected bool running;
        protected bool ignoreUnknown;
        protected bool isSecure = false;
        protected Thread thread;
        protected NetworkStream stream;
        protected BlockingCollection<Packet> output;
        private byte[] buffer = new byte[1024];
        private SslState sslState;
        private Ssl ssl;

        public event PacketHandler GotXNLXCMPPacket;

        public TCPClient(IPEndPoint ep) : this(ep, false)
        {
        }

        public TCPClient(IPEndPoint ep, bool ignoreUnknown)
        {
            this.ep = ep;
            this.ignoreUnknown = ignoreUnknown;
            rawClient = new TcpClient();
            rawClient.Connect(ep);
            stream = rawClient.GetStream();
            output = new BlockingCollection<Packet>();
            running = true;
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(GotData), null);
        }

        public void SecureUpgrade(IPEndPoint ep)
        {
            // Close the old stream and client
            stream.Flush();
            stream.Close();
            rawClient.Close();

            // Create a new TcpClient and NetworkStream for secure connection
            this.ep = ep;
            rawClient = new TcpClient();
            rawClient.Connect(ep);
            stream = rawClient.GetStream();
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(GotData), null);
            isSecure = true;

            // Set up SSL
            var options = new SslOptions();
            options.Ciphers = ["PSK-AES128-CBC-SHA", "PSK-AES256-CBC-SHA", "AES256-SHA256"];
            options.SslProtocol = SslProtocol.Tls12;
            options.SslStrength = SslStrength.Level0;
            options.ClientCertificateCallbackHandler = new ClientCertificateCallbackHandler(clientCertificateCallbackHandler);
            ssl = Ssl.CreateClientSsl(options);

            // Do the SSL handshake
            DoSslHandshake();
        }

        private void GotData(IAsyncResult result)
        {
            try
            {
                int count = stream.EndRead(result);
                if (count > 0)
                {
                    byte[] tmpBuffer = new byte[count];
                    Buffer.BlockCopy(buffer, 0, tmpBuffer, 0, count);
                    if (isSecure)
                    {
                        GotTLSData(tmpBuffer);
                    }
                    else
                    {
                        ProcessPacket(tmpBuffer);
                    }
                }
                stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(GotData), null);
            }
            catch (ObjectDisposedException)
            {
                //TCPClient is being disposed, this is fine, just ignore this exception
            }
        }

        private void ProcessPacket(byte[] buffer)
        {
            XNLPacket p = XNLPacket.Decode(buffer);
            XNLXCMPPacket pkt = new XNLXCMPPacket(new RadioID(0), p);
            log.DebugFormat("Received {0}", pkt);
            PacketEventArgs e = new PacketEventArgs(pkt, ep);
            if (GotXNLXCMPPacket != null)
            {
                GotXNLXCMPPacket(this, e);
            }
            else if (!ignoreUnknown)
            {
                log.ErrorFormat("Got an unknown packet {0}", p);
                output.Add(pkt);
                thread = new Thread(SendOld);
                thread.Start();
            }
        }

        private void GotTLSData(byte[] buffer)
        {
            byte[] writeBuffer = new byte[16383];
            sslState = ssl.ReadSsl(buffer, writeBuffer, out int readCount, out int writeCount);
            if (writeCount > 0)
            {
                ProcessPacket(writeBuffer.AsSpan(0, writeCount).ToArray());
            }
        }

        private void WriteSslCycle()
        {
            byte[] writeBuffer = new byte[16384];
            while (sslState.WantsWrite())
            {
                sslState = ssl.WriteSsl(ReadOnlySpan<byte>.Empty, writeBuffer, out int readCount, out int writeCount);
                if (writeCount > 0)
                {
                    stream.Write(writeBuffer, 0, writeCount);
                }
            }
        }

        private void WaitForRead()
        {
            while (sslState.WantsRead())
            {
                Thread.Sleep(10);
            }
        }

        private void DoSslHandshake()
        {
            while (!ssl.DoHandshake(out sslState))
            {
                if (sslState.WantsWrite())
                {
                    WriteSslCycle();
                }
            }
        }

        public void SendOld()
        {
            while (GotXNLXCMPPacket == null)
            {
                //Wait for an event listener to register...
                Thread.Sleep(100);
            }
            while (output.Count > 0)
            {
                Packet p = output.Take();
                PacketEventArgs e = new PacketEventArgs(p, ep);
                GotXNLXCMPPacket(this, e);
            }
        }

        public bool Send(XNLPacket packet)
        {
            byte[] bytes;
            log.DebugFormat("Sending packet {0} to {1}", packet, ep);
            bytes = packet.Encode();
            if (isSecure)
            {
                byte[] writeBuffer = new byte[16384];
                sslState = ssl.WriteSsl(bytes, writeBuffer, out int readCount, out int writeCount);
                if (writeCount > 0)
                {
                    stream.Write(writeBuffer, 0, writeCount);
                }
            }
            else
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            return true;
        }

        public bool RawSend(byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
            return true;
        }

        private bool clientCertificateCallbackHandler(IReadOnlyCollection<X509Name> validCA, out X509Certificate clientCertificate, out PrivateKey clientPrivateKey)
        {
            var cpsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Motorola", "MOTOTRBO CPS 2.0");
            var certPath = Path.Combine(cpsPath, "Metadata", "PCR", "resources", "msi_pcr.crt");
            if (!File.Exists(certPath))
            {
                certPath = Path.Combine("msi_pcr.crt");
            }
            var keyPath = Path.Combine(cpsPath, "Metadata", "PCR", "resources", "msi_pcr.pem");
            if (!File.Exists(keyPath))
            {
                keyPath = Path.Combine("msi_pcr.pem");
            }
            string password = ConfigurationManager.AppSettings.Get("keyPassword");
            clientCertificate = X509Certificate.Read(certPath, null);
            clientPrivateKey = PrivateKey.Read(keyPath, password);
            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (thread != null)
                {
                    thread.Abort();
                }
                if (disposing)
                {
                    rawClient.Close();
                    output.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
