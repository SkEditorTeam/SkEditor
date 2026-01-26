using System;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DiscordRPC.Helper
{
    internal class BackoffDelay(int min, int max, Random random)
    {
        /// <summary>
        /// The maximum time the backoff can reach
        /// </summary>
        public int Maximum { get; } = max;

        /// <summary>
        /// The minimum time the backoff can start at
        /// </summary>
        public int Minimum { get; } = min;

        /// <summary>
        /// The current time of the backoff
        /// </summary>
        public int Current { get; private set; } = min;

        /// <summary>
        /// The current number of failures
        /// </summary>
        public int Fails { get; private set; }

        /// <summary>
        /// The random generator
        /// </summary>
        public Random Random { get; set; } = random;

        public BackoffDelay(int min, int max) : this(min, max, new Random()) { }

        /// <summary>
        /// Resets the backoff
        /// </summary>
        public void Reset()
        {
            Fails = 0;
            Current = Minimum;
        }

        public int NextDelay()
        {
            // Increment the failures
            Fails++;

            double diff = (Maximum - Minimum) / 100f;
            Current = (int)Math.Floor(diff * Fails) + Minimum;

            return Math.Min(Math.Max(Current, Minimum), Maximum);
        }
    }
}
