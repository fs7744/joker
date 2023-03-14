using Microsoft.Extensions.Logging;

namespace Joker.Server
{
    internal sealed partial class JokerTrace : ILogger
    {
        private readonly ILogger _generalLogger;

        public JokerTrace(ILoggerFactory loggerFactory)
        {
            _generalLogger = loggerFactory.CreateLogger("Joker.Server");
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => _generalLogger.Log(logLevel, eventId, state, exception, formatter);

        public bool IsEnabled(LogLevel logLevel) => _generalLogger.IsEnabled(logLevel);

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _generalLogger.BeginScope(state);

        public void HeartbeatSlow(TimeSpan heartbeatDuration, TimeSpan interval, DateTimeOffset now)
        {
            // while the heartbeat does loop over connections, this log is usually an indicator of threadpool starvation
            GeneralLog.HeartbeatSlow(_generalLogger, now, heartbeatDuration, interval);
        }

        public void ApplicationNeverCompleted(string connectionId)
        {
            GeneralLog.ApplicationNeverCompleted(_generalLogger, connectionId);
        }

        private static partial class GeneralLog
        {
            [LoggerMessage(22, LogLevel.Warning, @"As of ""{now}"", the heartbeat has been running for ""{heartbeatDuration}"" which is longer than ""{interval}"". This could be caused by thread pool starvation.", EventName = "HeartbeatSlow")]
            public static partial void HeartbeatSlow(ILogger logger, DateTimeOffset now, TimeSpan heartbeatDuration, TimeSpan interval);

            [LoggerMessage(23, LogLevel.Critical, @"Connection id ""{ConnectionId}"" application never completed.", EventName = "ApplicationNeverCompleted")]
            public static partial void ApplicationNeverCompleted(ILogger logger, string connectionId);
        }

        #region Connection

        public void NotAllConnectionsClosedGracefully()
        {
            ConnectionsLog.NotAllConnectionsClosedGracefully(_generalLogger);
        }

        public void NotAllConnectionsAborted()
        {
            ConnectionsLog.NotAllConnectionsAborted(_generalLogger);
        }

        public void ConnectionStart(string connectionId)
        {
            ConnectionsLog.ConnectionStart(_generalLogger, connectionId);
        }

        public void ConnectionStop(string connectionId)
        {
            ConnectionsLog.ConnectionStop(_generalLogger, connectionId);
        }

        public void ConnectionAccepted(string connectionId)
        {
            ConnectionsLog.ConnectionAccepted(_generalLogger, connectionId);
        }

        private static partial class ConnectionsLog
        {
            [LoggerMessage(1, LogLevel.Debug, @"Connection id ""{ConnectionId}"" started.", EventName = "ConnectionStart")]
            public static partial void ConnectionStart(ILogger logger, string connectionId);

            [LoggerMessage(2, LogLevel.Debug, @"Connection id ""{ConnectionId}"" stopped.", EventName = "ConnectionStop")]
            public static partial void ConnectionStop(ILogger logger, string connectionId);

            [LoggerMessage(16, LogLevel.Debug, "Some connections failed to close gracefully during server shutdown.", EventName = "NotAllConnectionsClosedGracefully")]
            public static partial void NotAllConnectionsClosedGracefully(ILogger logger);

            [LoggerMessage(21, LogLevel.Debug, "Some connections failed to abort during server shutdown.", EventName = "NotAllConnectionsAborted")]
            public static partial void NotAllConnectionsAborted(ILogger logger);

            [LoggerMessage(39, LogLevel.Debug, @"Connection id ""{ConnectionId}"" accepted.", EventName = "ConnectionAccepted")]
            public static partial void ConnectionAccepted(ILogger logger, string connectionId);
        }

        #endregion Connection
    }
}