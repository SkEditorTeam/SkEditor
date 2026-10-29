using System.Threading;
using System.Threading.Tasks;
using SkEditor.Analyzing.Models;

namespace SkEditor.Analyzing;

/// <summary>
///     Manages everything related to the Analyzer functionality.
/// </summary>
public interface IAnalyzer
{
    /// <summary>
    ///     Gets the root directory of the Analyzer.
    /// </summary>
    string AnalyzerRoot { get; }

    /// <summary>
    ///     Gets the directory where the Analyzer binaries are located.
    /// </summary>
    string BinDir { get; }

    /// <summary>
    ///     Gets a value indicating whether the Analyzer is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    ///     Gets the working directory where the Analyzer server runs.
    /// </summary>
    string WorkingDir { get; }

    /// <summary>
    ///     Initializes the Analyzer.
    /// </summary>
    Task Initialize();

    /// <summary>
    ///     Analyzes the given text and returns the result.
    /// </summary>
    /// <remarks>A cancellation token is required for making sure only one analysis is running at a time.</remarks>
    Task<AnalyzerResult?> Analyze(string text, CancellationToken ct);
}