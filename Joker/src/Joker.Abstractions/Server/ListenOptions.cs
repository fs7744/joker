using Joker.Connections;
using System.Net;

namespace Joker.Server
{
    public class ListenOptions
    {
        public string Key { get; set; }

        public IReadOnlyCollection<EndPoint> EndPoints { get; set; }

        public ConnectionDelegate ConnectionDelegate { get; set; }
    }
}