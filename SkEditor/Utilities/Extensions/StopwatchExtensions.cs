using System.Diagnostics;

namespace SkEditor.Utilities.Extensions;

public static class StopwatchExtensions
{
    extension(Stopwatch stopwatch)
    {
        public double ElapsedMicroseconds => stopwatch.ElapsedTicks * 1_000_000.0 / Stopwatch.Frequency;
    }
}