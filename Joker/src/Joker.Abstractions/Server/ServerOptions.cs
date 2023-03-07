namespace Joker.Server
{
    public class ServerOptions
    {
        public long? MaxConcurrentUpgradedConnections { get; set; }

        public List<ListenOptions> ListenOptions { get; set; }
    }
}