using Joker.Server;
using Microsoft.Extensions.Hosting;

namespace Joker
{
    public class HostedService : IHostedService, IAsyncDisposable
    {
        private readonly IServer server;

        public HostedService(IServer server)
        {
            this.server = server;
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync(new CancellationToken(canceled: true)).ConfigureAwait(false);
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            return server.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            return server.StopAsync(cancellationToken);
        }
    }
}