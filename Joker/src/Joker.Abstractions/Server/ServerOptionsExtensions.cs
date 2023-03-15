using Joker.Connections;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Sockets;

namespace Joker.Server
{
    public static class ServerOptionsExtensions
    {
        public static ListenOptionsBuilder AddEndPoint(this ServerOptionsBuilder builder, string key)
        {
            ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));
            var lo = new ListenOptionsBuilder(key);
            builder.ListenOptionsBuilders.Add(lo);
            return lo;
        }

        public static ListenOptionsBuilder Listen(this ListenOptionsBuilder builder, params EndPoint[] endPoints)
        {
            ArgumentNullException.ThrowIfNull(endPoints, nameof(endPoints));
            if (endPoints.Length == 0)
            {
                throw new ArgumentException("Can't be empty", nameof(endPoints));
            }
            builder.EndPoints.AddRange(endPoints);
            return builder;
        }

        public static ListenOptionsBuilder ListenUdp(this ListenOptionsBuilder builder, params UdpEndPoint[] endPoints)
        {
            return builder.Listen(endPoints);
        }

        public static ListenOptionsBuilder ListenUnixDomainSocket(this ListenOptionsBuilder builder, params UnixDomainSocketEndPoint[] endPoints)
        {
            return builder.Listen(endPoints);
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