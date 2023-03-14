using Joker.Connections;
using Joker.Exceptions;
using System.IO.Pipelines;
using System.Net;

namespace Joker.Server
{
    internal sealed class TransportManager
    {
        private readonly List<ActiveTransport> _transports = new List<ActiveTransport>();

        private readonly List<IConnectionListenerFactory> _transportFactories;
        private readonly ServiceContext _serviceContext;

        public TransportManager(
            List<IConnectionListenerFactory> transportFactories,
            ServiceContext serviceContext)
        {
            _transportFactories = transportFactories;
            _serviceContext = serviceContext;
        }

        private ConnectionManager ConnectionManager => _serviceContext.ConnectionManager;
        private JokerTrace Trace => _serviceContext.Log;

        public async Task<EndPoint> BindAsync(EndPoint endPoint, ConnectionDelegate connectionDelegate, CancellationToken cancellationToken)
        {
            if (_transportFactories.Count == 0)
            {
                throw new InvalidOperationException($"Cannot bind with {nameof(ConnectionDelegate)} no {nameof(IConnectionListenerFactory)} is registered.");
            }

            foreach (var transportFactory in _transportFactories)
            {
                if (transportFactory.CanBind(endPoint))
                {
                    var transport = await transportFactory.BindAsync(endPoint, cancellationToken).ConfigureAwait(false);
                    StartAcceptLoop(new GenericConnectionListener(transport), c => connectionDelegate(c));
                    return transport.EndPoint;
                }
            }

            throw new InvalidOperationException($"No registered {nameof(IConnectionListenerFactory)} supports endpoint {endPoint.GetType().Name}: {endPoint}");
        }

        /// <summary>
        /// TlsHandshakeCallbackContext.Connection is ConnectionContext but QUIC connection only implements BaseConnectionContext.
        /// </summary>
        private sealed class ConnectionContextAdapter : ConnectionContext
        {
            private readonly ConnectionContext _inner;

            public ConnectionContextAdapter(ConnectionContext inner) => _inner = inner;

            public override IDuplexPipe Transport
            {
                get => _inner.Transport;
                set => _inner.Transport = value;
            }

            public override string ConnectionId
            {
                get => _inner.ConnectionId;
                set => _inner.ConnectionId = value;
            }

            public override IDictionary<object, object?> Items
            {
                get => _inner.Items;
                set => _inner.Items = value;
            }

            public override EndPoint? LocalEndPoint
            {
                get => _inner.LocalEndPoint;
                set => _inner.LocalEndPoint = value;
            }

            public override EndPoint? RemoteEndPoint
            {
                get => _inner.RemoteEndPoint;
                set => _inner.RemoteEndPoint = value;
            }

            public override CancellationToken ConnectionClosed
            {
                get => _inner.ConnectionClosed;
                set => _inner.ConnectionClosed = value;
            }

            public override IServiceProvider ServiceProvider => _inner.ServiceProvider;

            public override void Abort(ConnectionAbortedException abortReason)
            {
                _inner.Abort(abortReason);
            }

            public override ValueTask DisposeAsync() => _inner.DisposeAsync();
        }

        private void StartAcceptLoop(IConnectionListener connectionListener, Func<ConnectionContext, Task> connectionDelegate)
        {
            var transportConnectionManager = new TransportConnectionManager(_serviceContext.ConnectionManager);
            var connectionDispatcher = new ConnectionDispatcher(_serviceContext, connectionDelegate, transportConnectionManager);
            var acceptLoopTask = connectionDispatcher.StartAcceptingConnections(connectionListener);

            _transports.Add(new ActiveTransport(connectionListener, acceptLoopTask, transportConnectionManager));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return StopTransportsAsync(new List<ActiveTransport>(_transports), cancellationToken);
        }

        private async Task StopTransportsAsync(List<ActiveTransport> transportsToStop, CancellationToken cancellationToken)
        {
            var tasks = new Task[transportsToStop.Count];

            for (int i = 0; i < transportsToStop.Count; i++)
            {
                tasks[i] = transportsToStop[i].UnbindAsync(cancellationToken);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            async Task StopTransportConnection(ActiveTransport transport)
            {
                if (!await transport.TransportConnectionManager.CloseAllConnectionsAsync(cancellationToken).ConfigureAwait(false))
                {
                    Trace.NotAllConnectionsClosedGracefully();

                    if (!await transport.TransportConnectionManager.AbortAllConnectionsAsync().ConfigureAwait(false))
                    {
                        Trace.NotAllConnectionsAborted();
                    }
                }
            }

            for (int i = 0; i < transportsToStop.Count; i++)
            {
                tasks[i] = StopTransportConnection(transportsToStop[i]);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            for (int i = 0; i < transportsToStop.Count; i++)
            {
                tasks[i] = transportsToStop[i].DisposeAsync().AsTask();
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var transport in transportsToStop)
            {
                _transports.Remove(transport);
            }
        }

        private sealed class ActiveTransport : IAsyncDisposable
        {
            public ActiveTransport(IConnectionListener transport, Task acceptLoopTask, TransportConnectionManager transportConnectionManager)
            {
                ConnectionListener = transport;
                AcceptLoopTask = acceptLoopTask;
                TransportConnectionManager = transportConnectionManager;
            }

            public IConnectionListener ConnectionListener { get; }
            public Task AcceptLoopTask { get; }
            public TransportConnectionManager TransportConnectionManager { get; }

            public async Task UnbindAsync(CancellationToken cancellationToken)
            {
                await ConnectionListener.UnbindAsync(cancellationToken).ConfigureAwait(false);
                await AcceptLoopTask.ConfigureAwait(false);
            }

            public ValueTask DisposeAsync()
            {
                return ConnectionListener.DisposeAsync();
            }
        }

        private sealed class GenericConnectionListener : IConnectionListener
        {
            private readonly IConnectionListener _connectionListener;

            public GenericConnectionListener(IConnectionListener connectionListener)
            {
                _connectionListener = connectionListener;
            }

            public EndPoint EndPoint => _connectionListener.EndPoint;

            public ValueTask<ConnectionContext?> AcceptAsync(CancellationToken cancellationToken = default)
                 => _connectionListener.AcceptAsync(cancellationToken);

            public ValueTask UnbindAsync(CancellationToken cancellationToken = default)
                => _connectionListener.UnbindAsync(cancellationToken);

            public ValueTask DisposeAsync()
                => _connectionListener.DisposeAsync();
        }
    }
}