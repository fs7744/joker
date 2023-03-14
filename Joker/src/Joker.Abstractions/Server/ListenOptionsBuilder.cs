using Joker.Connections;
using System.Net;

namespace Joker.Server
{
    public class ListenOptionsBuilder
    {
        private readonly string key;
        public List<EndPoint> EndPoints { get; } = new List<EndPoint>();
        public List<Func<ConnectionDelegate, ConnectionDelegate>> Middlewares { get; } = new List<Func<ConnectionDelegate, ConnectionDelegate>>();
        public IServiceProvider ServiceProvider { get; private set; }

        public ListenOptionsBuilder(string key)
        {
            this.key = key;
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

            return new ListenOptions() { Key = key, EndPoints = EndPoints, ConnectionDelegate = app };
        }
    }
}