﻿using Joker.Connections;
using Joker.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace TcpDemo
{
    public class TestProxyHandler : IMiddleware
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly IPEndPoint proxyServer = new(IPAddress.Parse("14.215.177.38"), 80);
        private ConnectionContext upstream;

        public TestProxyHandler(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task Invoke(ConnectionContext connection, ConnectionDelegate next)
        {
            upstream = await connectionFactory.ConnectAsync(proxyServer);
            var task1 = connection.Transport.Input.CopyToAsync(upstream.Transport.Output);
            var task2 = upstream.Transport.Input.CopyToAsync(connection.Transport.Output);
            await Task.WhenAny(task1, task2);
            await next(connection);
        }
    }

    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureJoker(i =>
                {
                    i.Services.AddSingleton<TestProxyHandler>();
                    i.AddEndPoint("test").Listen(IPEndPoint.Parse("127.0.0.1:5000")).UseMiddleware<TestProxyHandler>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}