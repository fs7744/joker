﻿namespace Joker.Common
{
    public interface ISystemClock
    {
        /// <summary>
        /// Retrieves the current UTC system time.
        /// </summary>
        DateTimeOffset UtcNow { get; }

        /// <summary>
        /// Retrieves ticks for the current UTC system time.
        /// </summary>
        long UtcNowTicks { get; }

        /// <summary>
        /// Retrieves the current UTC system time.
        /// This is only safe to use from code called by the <see cref="Heartbeat"/>.
        /// </summary>
        DateTimeOffset UtcNowUnsynchronized { get; }
    }
}