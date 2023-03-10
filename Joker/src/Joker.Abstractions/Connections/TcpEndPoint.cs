using System.Net;

namespace Joker.Connections
{
    public class TcpEndPoint : IPEndPoint
    {
        public TcpEndPoint(long address, int port) : base(address, port)
        {
        }
    }
}