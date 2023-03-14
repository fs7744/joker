using System.Net;

namespace Joker.Connections
{
    public class UdpEndPoint : IPEndPoint
    {
        public UdpEndPoint(long address, int port) : base(address, port)
        {
        }
    }
}