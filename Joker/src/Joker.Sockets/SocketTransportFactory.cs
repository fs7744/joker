using Joker.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;

namespace Joker.Sockets
{
    public sealed class SocketTransportFactory : IConnectionListenerFactory
    {
        private readonly SocketTransportOptions _options;
        private readonly ILoggerFactory _logger;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketTransportFactory"/> class.
        /// </summary>
        /// <param name="options">The transport options.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public SocketTransportFactory(
            IOptions<SocketTransportOptions> options,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(loggerFactory);

            _options = options.Value;
            _logger = loggerFactory;
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            var transport = new SocketConnectionListener(endpoint, _options, _logger, serviceProvider);
            transport.Bind();
            return new ValueTask<IConnectionListener>(transport);
        }

        /// <inheritdoc />
        public bool CanBind(EndPoint endpoint)
        {
            return endpoint switch
            {
                IPEndPoint _ => true,
                UnixDomainSocketEndPoint _ => true,
                FileHandleEndPoint _ => true,
                _ => false
            };
        }
    }
}