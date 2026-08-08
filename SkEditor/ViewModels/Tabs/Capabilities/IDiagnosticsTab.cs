using System.Collections.Generic;
using ObservableCollections;
using SkEditor.Models.Analyzer;

namespace SkEditor.ViewModels.Tabs.Capabilities;

public interface IDiagnosticsTab
{
    ObservableList<AnalyzerError> Diagnostics { get; }
    Dictionary<int, AnalyzerError> DiagnosticDictionary { get; }
}