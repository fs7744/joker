using System.Net;

namespace Joker.Connections
{
    public interface IConnectionFactory<T> : IAsyncDisposable where T : EndPointData
    {
        ValueTask<ConnectionContext<T>> ConnectAsync(T endpoint, CancellationToken cancellationToken = default);
    }
}