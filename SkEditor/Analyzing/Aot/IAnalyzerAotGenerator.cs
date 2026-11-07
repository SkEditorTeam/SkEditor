using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Analyzing.Aot;

/// <summary>
///     Manages the AOT cache for the Analyzer, including generating it if necessary and removing it if it is corrupt.
/// </summary>
public interface IAnalyzerAotGenerator
{
    /// <summary>
    ///     Generates the AOT cache for the Analyzer.
    /// </summary>
    /// <param name="workingDir">The working "server" directory for the Analyzer.</param>
    /// <param name="javaPath">The path to the JRE executable file.</param>
    /// <param name="analyzerJarPath">The path to the Analyzer JAR file.</param>
    /// <param name="aotPath">The path to the AOT cache file.</param>
    /// <param name="libraryPaths">The paths to the libraries to include in the classpath.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    Task GenerateAotCacheAsync(string workingDir, string javaPath, string analyzerJarPath, string aotPath, string[] libraryPaths,
        CancellationToken ct = default);

    /// <summary>
    ///     Removes the AOT cache file if it exists.
    /// </summary>
    /// <param name="aotPath">The path to the AOT cache file.</param>
    /// <remarks>This method itself doesn't check if the cache is corrupt.</remarks>
    void CleanupCorruptAot(string aotPath);
}