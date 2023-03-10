using Joker.Connections;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Joker.Server
{
    public static class ServerOptionsExtensions
    {
        public static ListenOptionsBuilder AddEndPoint(this ServerOptionsBuilder builder, string key, params EndPoint[] endPoints)
        {
            ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));
            ArgumentNullException.ThrowIfNull(endPoints, nameof(endPoints));
            if (endPoints.Length == 0)
            {
                throw new ArgumentException("Can't be empty", nameof(endPoints));
            }
            var lo = new ListenOptionsBuilder(key, endPoints);
            builder.ListenOptionsBuilders.Add(lo);
            return lo;
        }

        public static ListenOptionsBuilder UseMiddleware(this ListenOptionsBuilder builder, Func<ConnectionDelegate, ConnectionDelegate> middleware)
        {
            builder.Middlewares.Add(middleware);
            return builder;
        }

        public static ListenOptionsBuilder UseMiddleware<T>(this ListenOptionsBuilder builder) where T : IMiddleware
        {
            builder.UseMiddleware(next =>
            {
                var serviceProvider = builder.ServiceProvider;
                var p = serviceProvider.GetRequiredService<T>();
                return c => p.Invoke(c, next);
            });
            return builder;
        }
    }
}