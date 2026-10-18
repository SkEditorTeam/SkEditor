using System.Text.RegularExpressions;
using SkEditor.Core.Logging;

namespace SkEditor.Analyzing;

public static partial class AnalyzerLogExtensions
{
    public static void LogAnalyzerLine(this ILogService logs, string? rawLine)
    {
        if (string.IsNullOrWhiteSpace(rawLine))
        {
            return;
        }

        Match match = AnalyzerLogHeaderRegex().Match(rawLine);

        if (!match.Success)
        {
            logs.Info(rawLine, LogSource.Analyzer);
            return;
        }

        string levelStr = match.Groups["level"].Value;
        string cleanMessage = match.Groups["message"].Value;

        switch (levelStr)
        {
            case "TRACE":
            case "DEBUG":
                logs.Debug(cleanMessage, LogSource.Analyzer);
                break;

            case "INFO":
                logs.Info(cleanMessage, LogSource.Analyzer);
                break;

            case "WARN":
            case "WARNING":
                logs.Warning(cleanMessage, LogSource.Analyzer);
                break;

            case "ERROR":
            case "SEVERE":
            case "FATAL":
                logs.Error(cleanMessage, LogSource.Analyzer);
                break;

            default:
                logs.Info(cleanMessage, LogSource.Analyzer);
                break;
        }
    }

    [GeneratedRegex(@"^\[\d{2}:\d{2}:\d{2}\]\s+(?<level>[A-Z]+)\s+(?<message>.*)$")]
    private static partial Regex AnalyzerLogHeaderRegex();
}