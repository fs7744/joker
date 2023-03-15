using Joker.Connections;
using Joker.Server;

namespace Joker.Protocols.Tls
{
    public class TlsConnectionMiddleware : IMiddleware
    {
        public Task Invoke(ConnectionContext connection, ConnectionDelegate next)
        {
            throw new NotImplementedException();
        }
    }
}