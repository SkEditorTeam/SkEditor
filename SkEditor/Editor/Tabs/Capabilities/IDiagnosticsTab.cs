using System.Collections.Generic;
using ObservableCollections;
using SkEditor.Models.Analyzer;

namespace SkEditor.ViewModels.Tabs.Capabilities;

public interface IDiagnosticsTab
{
    Dictionary<int, AnalyzerError> DiagnosticDictionary { get; }
    ObservableList<AnalyzerError> Diagnostics { get; }
}