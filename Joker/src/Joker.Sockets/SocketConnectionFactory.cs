using Joker.Connections;
using Joker.Sockets.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace Joker.Sockets
{
    public sealed class SocketConnectionFactory : IConnectionFactory, IAsyncDisposable
    {
        private readonly SocketTransportOptions _options;
        private readonly MemoryPool<byte> _memoryPool;
        private readonly ILogger _trace;
        private readonly PipeOptions _inputOptions;
        private readonly PipeOptions _outputOptions;
        private readonly SocketSenderPool _socketSenderPool;
        private readonly IServiceProvider serviceProvider;

        public SocketConnectionFactory(IOptions<SocketTransportOptions> options, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(loggerFactory);

            _options = options.Value;
            _memoryPool = options.Value.MemoryPoolFactory();
            _trace = loggerFactory.CreateLogger<SocketConnectionFactory>();

            var maxReadBufferSize = _options.MaxReadBufferSize ?? 0;
            var maxWriteBufferSize = _options.MaxWriteBufferSize ?? 0;

            // These are the same, it's either the thread pool or inline
            var applicationScheduler = _options.UnsafePreferInlineScheduling ? PipeScheduler.Inline : PipeScheduler.ThreadPool;
            var transportScheduler = applicationScheduler;
            // https://github.com/aspnet/KestrelHttpServer/issues/2573
            var awaiterScheduler = OperatingSystem.IsWindows() ? transportScheduler : PipeScheduler.Inline;

            _inputOptions = new PipeOptions(_memoryPool, applicationScheduler, transportScheduler, maxReadBufferSize, maxReadBufferSize / 2, useSynchronizationContext: false);
            _outputOptions = new PipeOptions(_memoryPool, transportScheduler, applicationScheduler, maxWriteBufferSize, maxWriteBufferSize / 2, useSynchronizationContext: false);
            _socketSenderPool = new SocketSenderPool(awaiterScheduler);
            this.serviceProvider = serviceProvider;
        }

        public async ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            var ipEndPoint = endpoint as IPEndPoint ?? throw new NotSupportedException("The SocketConnectionFactory only supports IPEndPoints for now.");
            var socket = _options.CreateBoundSocket(endpoint);
            socket.NoDelay = _options.NoDelay;

            await socket.ConnectAsync(ipEndPoint);

            var socketConnection = new SocketConnection(
                socket,
                _memoryPool,
                _inputOptions.ReaderScheduler, // This is either threadpool or inline
                _trace,
                _socketSenderPool,
                _inputOptions,
                _outputOptions,
                serviceProvider,
                _options.WaitForDataBeforeAllocatingBuffer);

            socketConnection.Start();
            return socketConnection;
        }

        public ValueTask DisposeAsync()
        {
            _memoryPool.Dispose();
            return default;
        }
    }
}