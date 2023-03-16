using Joker.Exceptions;
using Microsoft.AspNetCore.Http.Features;
using System.IO.Pipelines;
using System.Net;

namespace Joker.Connections
{
    public abstract class ConnectionContext
    {
        public abstract IDuplexPipe Transport { get; set; }

        public abstract string ConnectionId { get; set; }

        public abstract IServiceProvider ServiceProvider { get; }

        public abstract IDictionary<object, object?> Items { get; set; }

        public abstract IFeatureCollection Features { get; }

        public virtual CancellationToken ConnectionClosed { get; set; }

        public virtual EndPoint? LocalEndPoint { get; set; }
        public virtual EndPoint? RemoteEndPoint { get; set; }

        public abstract void Abort(ConnectionAbortedException abortReason);

        public abstract ValueTask DisposeAsync();
    }
}