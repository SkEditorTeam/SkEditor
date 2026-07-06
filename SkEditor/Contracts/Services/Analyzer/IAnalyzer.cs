using System.Threading;
using System.Threading.Tasks;
using SkEditor.Models.Analyzer;

namespace SkEditor.Contracts.Services.Analyzer;

public interface IAnalyzer
{
    Task Initialize();
    Task<AnalyzerResult?> Analyze(string text, CancellationToken ct);
}