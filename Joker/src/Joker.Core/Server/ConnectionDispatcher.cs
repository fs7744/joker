using Joker.Connections;
using Microsoft.Extensions.Logging;

namespace Joker.Server
{
    internal sealed class ConnectionDispatcher
    {
        private readonly ServiceContext _serviceContext;
        private readonly Func<ConnectionContext, Task> _connectionDelegate;
        private readonly TransportConnectionManager _transportConnectionManager;
        private readonly TaskCompletionSource _acceptLoopTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        public ConnectionDispatcher(ServiceContext serviceContext, Func<ConnectionContext, Task> connectionDelegate, TransportConnectionManager transportConnectionManager)
        {
            _serviceContext = serviceContext;
            _connectionDelegate = connectionDelegate;
            _transportConnectionManager = transportConnectionManager;
        }

        private JokerTrace Log => _serviceContext.Log;

        public Task StartAcceptingConnections(IConnectionListener listener)
        {
            ThreadPool.UnsafeQueueUserWorkItem(StartAcceptingConnectionsCore, listener, preferLocal: false);
            return _acceptLoopTcs.Task;
        }

        private void StartAcceptingConnectionsCore(IConnectionListener listener)
        {
            // REVIEW: Multiple accept loops in parallel?
            _ = AcceptConnectionsAsync();

            async Task AcceptConnectionsAsync()
            {
                try
                {
                    while (true)
                    {
                        var connection = await listener.AcceptAsync();

                        if (connection == null)
                        {
                            // We're done listening
                            break;
                        }

                        // Add the connection to the connection manager before we queue it for execution
                        var id = _transportConnectionManager.GetNewConnectionId();
                        var kestrelConnection = new JokerConnection<ConnectionContext>(
                            id, _serviceContext, _transportConnectionManager, _connectionDelegate, connection, Log);

                        _transportConnectionManager.AddConnection(id, kestrelConnection);

                        Log.ConnectionAccepted(connection.ConnectionId);

                        ThreadPool.UnsafeQueueUserWorkItem(kestrelConnection, preferLocal: false);
                    }
                }
                catch (Exception ex)
                {
                    // REVIEW: If the accept loop ends should this trigger a server shutdown? It will manifest as a hang
                    Log.LogCritical(0, ex, "The connection listener failed to accept any new connections.");
                }
                finally
                {
                    _acceptLoopTcs.TrySetResult();
                }
            }
        }
    }
}