using System.Net;

namespace Joker.Connections
{
    public interface IConnectionListenerFactory
    {
        bool CanBind(EndPoint endpoint);

        ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default);
    }
}