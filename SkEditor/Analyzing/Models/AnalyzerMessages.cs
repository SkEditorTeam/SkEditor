using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Analyzing.Models;

public record AnalyzerLoadedMessage;

public record AnalyzerUnloadedMessage;

public record AnalyzerFailedToLoadMessage;

public record AnalyzerStatusChangedMessage(bool IsAnalyzing);

public record AnalyzerResultUpdatedMessage(IFileTab Tab, AnalyzerResult Result);