using System;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Models.Analyzer;

namespace SkEditor.Analyzing.Protocol;

/// <summary>
///     Manages the communication with the Analyzer process, handling commands and responses.
/// </summary>
public interface IAnalyzerProtocolHandler
{
    /// <summary>
    ///     Sends a parse command to the Analyzer process for the specified script file and awaits the analysis results.
    /// </summary>
    /// <param name="scriptFilePath">The path to the script file to be analyzed.</param>
    /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
    /// <returns>The analysis results, or null if the operation was canceled or failed.</returns>
    Task<AnalyzeResultsDto?> SendParseCommandAsync(string scriptFilePath, CancellationToken ct);

    /// <summary>
    ///     Handles output data received from the Analyzer process.
    /// </summary>
    void HandleOutputData(string data);

    /// <summary>
    ///     Handles error data received from the Analyzer process.
    /// </summary>
    void HandleErrorData(string data);

    /// <summary>
    ///     Resets the internal state flags before starting a new Analyzer process.
    /// </summary>
    void ResetState();

    /// <summary>
    ///     Raised when the AOT cache file is detected to be corrupted, so it should be deleted.
    /// </summary>
    event EventHandler? CorruptedAotDetected;
}