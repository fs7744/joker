using Microsoft.Extensions.DependencyInjection;

namespace Joker.Server
{
    public class ServerOptionsBuilder
    {
        public ServerOptionsBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
        public List<ListenOptionsBuilder> ListenOptionsBuilders { get; } = new List<ListenOptionsBuilder>();

        public ServerOptions Build(IServiceProvider serviceProvider)
        {
            return new ServerOptions() { ListenOptions = ListenOptionsBuilders.Select(i => i.Build(serviceProvider)).ToList() };
        }
    }
}