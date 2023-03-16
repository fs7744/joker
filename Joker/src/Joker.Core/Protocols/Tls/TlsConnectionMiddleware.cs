using Joker.Connections;
using Joker.Server;

namespace Joker.Protocols.Tls
{
    public class TlsConnectionMiddleware : IMiddleware
    {
        public async Task Invoke(ConnectionContext context, ConnectionDelegate next)
        {
            if (context.Features.Get<ITlsConnectionFeature>() != null)
            {
                await next(context);
                return;
            }
        }
    }
}