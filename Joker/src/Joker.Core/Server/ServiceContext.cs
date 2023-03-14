using Joker.Common;
using System.IO.Pipelines;

namespace Joker.Server
{
    internal class ServiceContext
    {
        public JokerTrace Log { get; set; } = default!;

        public PipeScheduler Scheduler { get; set; } = default!;

        public ISystemClock SystemClock { get; set; } = default!;

        public ConnectionManager ConnectionManager { get; set; } = default!;

        public Heartbeat Heartbeat { get; set; } = default!;

        public ServerOptions ServerOptions { get; set; } = default!;
    }
}