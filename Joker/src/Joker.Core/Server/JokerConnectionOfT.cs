using Joker.Connections;
using Microsoft.Extensions.Logging;

namespace Joker.Server
{
    internal sealed class JokerConnection<T> : JokerConnection, IThreadPoolWorkItem where T : ConnectionContext
    {
        private readonly Func<T, Task> _connectionDelegate;
        private readonly T _transportConnection;

        public JokerConnection(long id,
                                 ServiceContext serviceContext,
                                 TransportConnectionManager transportConnectionManager,
                                 Func<T, Task> connectionDelegate,
                                 T connectionContext,
                                 JokerTrace logger)
            : base(id, serviceContext, transportConnectionManager, logger)
        {
            _connectionDelegate = connectionDelegate;
            _transportConnection = connectionContext;
        }

        public override ConnectionContext TransportConnection => _transportConnection;

        void IThreadPoolWorkItem.Execute()
        {
            _ = ExecuteAsync();
        }

        internal async Task ExecuteAsync()
        {
            var connectionContext = _transportConnection;

            try
            {
                Logger.ConnectionStart(connectionContext.ConnectionId);

                using (BeginConnectionScope(connectionContext))
                {
                    try
                    {
                        await _connectionDelegate(connectionContext);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(0, ex, "Unhandled exception while processing {ConnectionId}.", connectionContext.ConnectionId);
                    }
                }
            }
            finally
            {
                await FireOnCompletedAsync();

                Logger.ConnectionStop(connectionContext.ConnectionId);

                // Dispose the transport connection, this needs to happen before removing it from the
                // connection manager so that we only signal completion of this connection after the transport
                // is properly torn down.
                await connectionContext.DisposeAsync();

                _transportConnectionManager.RemoveConnection(_id);
            }
        }
    }
}