using Joker.Connections;
using System.Net;

namespace Joker.Server
{
    public class ListenOptionsBuilder
    {
        private readonly string key;
        private readonly EndPoint[] endPoints;
        public List<Func<ConnectionDelegate, ConnectionDelegate>> Middlewares { get; } = new List<Func<ConnectionDelegate, ConnectionDelegate>>();
        public IServiceProvider ServiceProvider { get; private set; }

        public ListenOptionsBuilder(string key, EndPoint[] endPoints)
        {
            this.key = key;
            this.endPoints = endPoints;
        }

        internal ListenOptions Build(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            ConnectionDelegate app = context =>
            {
                return Task.CompletedTask;
            };

            foreach (var component in Middlewares.Reverse<Func<ConnectionDelegate, ConnectionDelegate>>())
            {
                app = component(app);
            }

            return new ListenOptions() { Key = key, EndPoints = endPoints, ConnectionDelegate = app };
        }
    }
}