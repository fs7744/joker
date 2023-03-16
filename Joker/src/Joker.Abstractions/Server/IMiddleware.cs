using Joker.Connections;

namespace Joker.Server
{
    public interface IMiddleware
    {
        Task Invoke(ConnectionContext context, ConnectionDelegate next);
    }
}