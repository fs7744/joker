using Joker.Common;
using Joker.Connections;
using Microsoft.Extensions.Logging;
using System.IO.Pipelines;

namespace Joker.Server
{
    public class JokerServer : IServer
    {
        private bool _hasStarted;
        private int _stopping;
        private readonly SemaphoreSlim _bindSemaphore = new(initialCount: 1);
        private readonly CancellationTokenSource _stopCts = new();
        private readonly TaskCompletionSource _stoppedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly ServiceContext _serviceContext;
        private readonly TransportManager _transportManager;

        public JokerServer(ServerOptions serverOptions, IEnumerable<IConnectionListenerFactory> transportFactories, ILoggerFactory loggerFactory)
        {
            var trace = new JokerTrace(loggerFactory);
            var connectionManager = new ConnectionManager(
                trace,
                serverOptions.MaxConcurrentUpgradedConnections);

            var heartbeatManager = new HeartbeatManager(connectionManager);

            var heartbeat = new Heartbeat(
                new IHeartbeatHandler[] { heartbeatManager },
                new SystemClock(),
                trace);
            _serviceContext = new ServiceContext
            {
                Log = trace,
                Scheduler = PipeScheduler.ThreadPool,
                SystemClock = heartbeatManager,
                ConnectionManager = connectionManager,
                Heartbeat = heartbeat,
                ServerOptions = serverOptions
            };
            _transportManager = new TransportManager(transportFactories.ToList(), _serviceContext);
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync(new CancellationToken(canceled: true)).ConfigureAwait(false);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_hasStarted)
                {
                    throw new InvalidOperationException("Server already started");
                }
                _hasStarted = true;

                _serviceContext.Heartbeat?.Start();

                await BindAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await DisposeAsync();
                throw;
            }
        }

        private async Task BindAsync(CancellationToken cancellationToken)
        {
            await _bindSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (_stopping == 1)
                {
                    throw new InvalidOperationException("Zzz has already been stopped.");
                }

                foreach (var listenOptions in _serviceContext.ServerOptions.ListenOptions)
                {
                    foreach (var endPoint in listenOptions.EndPoints)
                    {
                        await _transportManager.BindAsync(endPoint, listenOptions.ConnectionDelegate, cancellationToken);
                    }
                }
            }
            finally
            {
                _bindSemaphore.Release();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (Interlocked.Exchange(ref _stopping, 1) == 1)
            {
                await _stoppedTcs.Task.ConfigureAwait(false);
                return;
            }

            _stopCts.Cancel();

#pragma warning disable CA2016 // Don't use cancellationToken when acquiring the semaphore. Dispose calls this with a pre-canceled token.
            await _bindSemaphore.WaitAsync().ConfigureAwait(false);
#pragma warning restore CA2016

            try
            {
                await _transportManager.StopAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _stoppedTcs.TrySetException(ex);
                throw;
            }
            finally
            {
                _serviceContext.Heartbeat?.Dispose();
                _stopCts.Dispose();
                _bindSemaphore.Release();
            }

            _stoppedTcs.TrySetResult();
        }
    }
}