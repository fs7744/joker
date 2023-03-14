﻿using Microsoft.Extensions.Logging;

namespace Joker.Sockets.Internal
{
    internal static partial class SocketsLog
    {
        // Reserved: Event ID 3, EventName = ConnectionRead

        [LoggerMessage(6, LogLevel.Debug, @"Connection id ""{ConnectionId}"" received FIN.", EventName = "ConnectionReadFin", SkipEnabledCheck = true)]
        private static partial void ConnectionReadFinCore(ILogger logger, string connectionId);

        public static void ConnectionReadFin(ILogger logger, SocketConnection connection)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                ConnectionReadFinCore(logger, connection.ConnectionId);
            }
        }

        [LoggerMessage(7, LogLevel.Debug, @"Connection id ""{ConnectionId}"" sending FIN because: ""{Reason}""", EventName = "ConnectionWriteFin", SkipEnabledCheck = true)]
        private static partial void ConnectionWriteFinCore(ILogger logger, string connectionId, string reason);

        public static void ConnectionWriteFin(ILogger logger, SocketConnection connection, string reason)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                ConnectionWriteFinCore(logger, connection.ConnectionId, reason);
            }
        }

        // Reserved: Event ID 11, EventName = ConnectionWrite

        // Reserved: Event ID 12, EventName = ConnectionWriteCallback

        [LoggerMessage(14, LogLevel.Debug, @"Connection id ""{ConnectionId}"" communication error.", EventName = "ConnectionError", SkipEnabledCheck = true)]
        private static partial void ConnectionErrorCore(ILogger logger, string connectionId, Exception ex);

        public static void ConnectionError(ILogger logger, SocketConnection connection, Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                ConnectionErrorCore(logger, connection.ConnectionId, ex);
            }
        }

        [LoggerMessage(19, LogLevel.Debug, @"Connection id ""{ConnectionId}"" reset.", EventName = "ConnectionReset", SkipEnabledCheck = true)]
        public static partial void ConnectionReset(ILogger logger, string connectionId);

        public static void ConnectionReset(ILogger logger, SocketConnection connection)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                ConnectionReset(logger, connection.ConnectionId);
            }
        }

        [LoggerMessage(4, LogLevel.Debug, @"Connection id ""{ConnectionId}"" paused.", EventName = "ConnectionPause", SkipEnabledCheck = true)]
        private static partial void ConnectionPauseCore(ILogger logger, string connectionId);

        public static void ConnectionPause(ILogger logger, SocketConnection connection)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                ConnectionPauseCore(logger, connection.ConnectionId);
            }
        }

        [LoggerMessage(5, LogLevel.Debug, @"Connection id ""{ConnectionId}"" resumed.", EventName = "ConnectionResume", SkipEnabledCheck = true)]
        private static partial void ConnectionResumeCore(ILogger logger, string connectionId);

        public static void ConnectionResume(ILogger logger, SocketConnection connection)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                ConnectionResumeCore(logger, connection.ConnectionId);
            }
        }
    }
}