using System.Diagnostics.CodeAnalysis;

namespace Joker.Server
{
    internal sealed class ConnectionReference
    {
        private readonly long _id;
        private readonly WeakReference<JokerConnection> _weakReference;
        private readonly TransportConnectionManager _transportConnectionManager;

        public ConnectionReference(long id, JokerConnection connection, TransportConnectionManager transportConnectionManager)
        {
            _id = id;

            _weakReference = new WeakReference<JokerConnection>(connection);
            ConnectionId = connection.TransportConnection.ConnectionId;

            _transportConnectionManager = transportConnectionManager;
        }

        public string ConnectionId { get; }

        public bool TryGetConnection([NotNullWhen(true)] out JokerConnection? connection)
        {
            return _weakReference.TryGetTarget(out connection);
        }

        public void StopTrasnsportTracking()
        {
            _transportConnectionManager.StopTracking(_id);
        }
    }
}