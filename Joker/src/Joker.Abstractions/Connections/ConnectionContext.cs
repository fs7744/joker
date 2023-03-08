using Joker.Exceptions;
using System.IO.Pipelines;

namespace Joker.Connections
{
    public abstract class ConnectionContext
    {
        public abstract IDuplexPipe Transport { get; set; }

        public abstract string ConnectionId { get; set; }

        public abstract IServiceProvider ServiceProvider { get; }

        public abstract IDictionary<object, object?> Items { get; set; }

        public virtual CancellationToken ConnectionClosed { get; set; }

        public abstract void Abort(ConnectionAbortedException abortReason);

        public abstract ValueTask DisposeAsync();
    }
}