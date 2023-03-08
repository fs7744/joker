using Joker.Exceptions;
using System.IO.Pipelines;

namespace Joker.Connections
{
    public abstract class ConnectionContext<T> : ConnectionContext where T : EndPointData
    {
        public virtual T? LocalEndPoint { get; set; }
        public virtual T? RemoteEndPoint { get; set; }
    }
}