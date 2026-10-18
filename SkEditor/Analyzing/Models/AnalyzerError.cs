namespace SkEditor.Analyzing.Models;

public record AnalyzerError(AnalyzerErrorType Type, string Message, int Line);