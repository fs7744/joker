namespace Joker.Server
{
    public interface IServer : IAsyncDisposable
    {
        Task StartAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}