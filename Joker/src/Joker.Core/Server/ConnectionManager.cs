using System.Collections.Concurrent;

namespace Joker.Server
{
    internal sealed class ConnectionManager
    {
        private long _lastConnectionId = long.MinValue;

        private readonly ConcurrentDictionary<long, ConnectionReference> _connectionReferences = new ConcurrentDictionary<long, ConnectionReference>();
        private readonly JokerTrace _trace;

        public ConnectionManager(JokerTrace trace, long? upgradedConnectionLimit)
            : this(trace, GetCounter(upgradedConnectionLimit))
        {
        }

        public ConnectionManager(JokerTrace trace, ResourceCounter upgradedConnections)
        {
            UpgradedConnectionCount = upgradedConnections;
            _trace = trace;
        }

        public long GetNewConnectionId() => Interlocked.Increment(ref _lastConnectionId);

        /// <summary>
        /// Connections that have been switched to a different protocol.
        /// </summary>
        public ResourceCounter UpgradedConnectionCount { get; }

        public void AddConnection(long id, ConnectionReference connectionReference)
        {
            if (!_connectionReferences.TryAdd(id, connectionReference))
            {
                throw new ArgumentException("Unable to add connection.", nameof(id));
            }
        }

        public void RemoveConnection(long id)
        {
            if (!_connectionReferences.TryRemove(id, out var reference))
            {
                throw new ArgumentException("Unable to remove connection.", nameof(id));
            }

            if (reference.TryGetConnection(out var connection))
            {
                connection.Complete();
            }
        }

        public void Walk(Action<JokerConnection> callback)
        {
            foreach (var kvp in _connectionReferences)
            {
                var reference = kvp.Value;

                if (reference.TryGetConnection(out var connection))
                {
                    callback(connection);
                }
                else if (_connectionReferences.TryRemove(kvp.Key, out reference))
                {
                    // It's safe to modify the ConcurrentDictionary in the foreach.
                    // The connection reference has become unrooted because the application never completed.
                    _trace.ApplicationNeverCompleted(reference.ConnectionId);
                    reference.StopTrasnsportTracking();
                }

                // If both conditions are false, the connection was removed during the heartbeat.
            }
        }

        private static ResourceCounter GetCounter(long? number)
            => number.HasValue
                ? ResourceCounter.Quota(number.Value)
                : ResourceCounter.Unlimited;
    }
}