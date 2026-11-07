using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Analyzing.Bootstrapping;

/// <summary>
///     Manages the bootstrapping process for the Analyzer, ensuring that necessary libraries are extracted.
/// </summary>
public interface IAnalyzerBootstrapper
{
    /// <summary>
    ///     Extracts the necessary libraries from the Analyzer JAR file to the specified working directory.
    /// </summary>
    /// <param name="analyzerJarPath">The path to the Analyzer JAR file.</param>
    /// <param name="workingDir">The working "server" directory for the Analyzer.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>Extracted library paths if successful and null otherwise.</returns>
    Task<string[]?> EnsureLibrariesExtractedAsync(string analyzerJarPath, string workingDir, CancellationToken ct = default);
}