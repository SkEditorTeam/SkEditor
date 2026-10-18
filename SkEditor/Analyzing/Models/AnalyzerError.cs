namespace SkEditor.Models.Analyzer;

public record AnalyzerError(AnalyzerErrorType Type, string Message, int Line);