using System.Net;

namespace Joker.Connections
{
    public interface IConnectionListenerFactory<T> where T : EndPointData
    {
        bool CanBind(T endpoint);

        ValueTask<IConnectionListener<T>> BindAsync(T endpoint, CancellationToken cancellationToken = default);
    }
}