using System.Threading;
using System.Threading.Tasks;
using SkEditor.Models.Analyzer;

namespace SkEditor.Analyzing;

/// <summary>
/// Manages everything related to the Analyzer functionality.
/// </summary>
public interface IAnalyzer
{
    /// <summary>
    /// Initializes the Analyzer.
    /// </summary>
    Task Initialize();
    
    /// <summary>
    /// Analyzes the given text and returns the result.
    /// </summary>
    Task<AnalyzerResult?> Analyze(string text, CancellationToken ct);
}