using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Models.Analyzer;

public record AnalyzerLoadedMessage;

public record AnalyzerStatusChangedMessage(bool IsAnalyzing);

public record AnalyzerResultUpdatedMessage(IFileTab Tab, AnalyzerResult Result);