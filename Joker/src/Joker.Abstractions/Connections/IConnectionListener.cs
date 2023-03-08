using System.Net;

namespace Joker.Connections
{
    public interface IConnectionListener : IAsyncDisposable
    {
        EndPoint EndPoint { get; }

        ValueTask<ConnectionContext?> AcceptAsync(CancellationToken cancellationToken = default);

        ValueTask UnbindAsync(CancellationToken cancellationToken = default);
    }
}