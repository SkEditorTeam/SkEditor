namespace SkEditor.Models.Analyzer;

public record AnalyzerError(AnalyzerErrorType Type, string Message, string File, int Line);