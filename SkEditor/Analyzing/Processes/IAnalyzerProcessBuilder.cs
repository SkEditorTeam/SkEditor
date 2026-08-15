using System.Diagnostics;

namespace SkEditor.Analyzing.Processes;

/// <summary>
///     Builds the ProcessStartInfo for the Analyzer process.
/// </summary>
public interface IAnalyzerProcessBuilder
{
    /// <summary>
    ///     Builds the ProcessStartInfo for the Analyzer process.
    /// </summary>
    /// <param name="workingDir">The working "server" directory for the Analyzer.</param>
    /// <param name="analyzerPath">The path to the Analyzer JAR file.</param>
    /// <param name="aotPath">The path to the AOT cache file.</param>
    /// <param name="isGeneratingAot"> Whether the process is generating AOT cache or just using it.</param>
    /// <returns></returns>
    ProcessStartInfo BuildProcessStartInfo(string workingDir, string analyzerPath, string? aotPath,
        bool isGeneratingAot = false);
}