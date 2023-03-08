using System.Net;

namespace Joker.Connections
{
    public interface IConnectionListener<T> : IAsyncDisposable where T : EndPointData
    {
        T EndPoint { get; }

        ValueTask<ConnectionContext<T>?> AcceptAsync(CancellationToken cancellationToken = default);

        ValueTask UnbindAsync(CancellationToken cancellationToken = default);
    }
}