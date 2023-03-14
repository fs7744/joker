namespace Joker.Server
{
    public interface IHeartbeatHandler
    {
        void OnHeartbeat(DateTimeOffset now);
    }
}