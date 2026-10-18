using System.Collections.Generic;
using ObservableCollections;
using SkEditor.Analyzing.Models;

namespace SkEditor.Editor.Tabs.Capabilities;

public interface IDiagnosticsTab
{
    Dictionary<int, AnalyzerError> DiagnosticDictionary { get; }
    ObservableList<AnalyzerError> Diagnostics { get; }
}