using System;
using System.Net.Sockets;

namespace AppLog.Logging.GrayLog
{
    internal class GrayLogUdpClient : GrayLogClient
    {
        public GrayLogUdpClient(string facility, string host, int port = 12201)
            : this(facility, new UdpClient(host, port), true)
        { }
        public GrayLogUdpClient(string facility, UdpClient udpClient)
            : this(facility, udpClient, false)
        { }

        protected GrayLogUdpClient(string facility, UdpClient udpClient, bool udpClientIsOwned)
            : base(facility)
        {
            this.UdpClient = udpClient;
            this.UdpClientIsOwned = udpClientIsOwned;
            this.NextChunckedMessageId = new Random().Next(0, Int32.MaxValue);
            this.MaxPacketSize = 1024;
        }
    }
}
