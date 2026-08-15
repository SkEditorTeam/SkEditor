using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SkEditor.Analyzing.Processes;

/// <summary>
///     Manages the lifecycle of the Analyzer process.
/// </summary>
public interface IAnalyzerProcessRunner : IDisposable
{
    /// <summary>
    ///     Whether the Analyzer process is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    ///     Starts the Analyzer process with the specified start information.
    /// </summary>
    /// <param name="startInfo">The start information for the Analyzer process.</param>
    void Start(ProcessStartInfo startInfo);

    /// <summary>
    ///     Writes a line of input to the Analyzer process's standard input asynchronously.
    /// </summary>
    Task WriteInputLineAsync(string input);

    /// <summary>
    ///     Stops the Analyzer process and releases any associated resources.
    /// </summary>
    void Stop();

    /// <summary>
    ///     Raised when the Analyzer process outputs data to standard output.
    /// </summary>
    event EventHandler<string>? OutputDataReceived;

    /// <summary>
    ///     Raised when the Analyzer process outputs data to standard error.
    /// </summary>
    event EventHandler<string>? ErrorDataReceived;

    /// <summary>
    ///     Raised when the Analyzer process exits.
    /// </summary>
    event EventHandler? ProcessExited;
}