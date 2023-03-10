using System.Net;

namespace Joker.Connections
{
    public interface IConnectionFactory : IAsyncDisposable 
    {
        ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default);
    }
}