using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Xaml.Behaviors.SourceGenerators;

[assembly: GenerateEventCommand(typeof(FATabViewItem), "CloseRequested")]
[assembly: GenerateEventCommand(typeof(ComboBox), "SelectionChanged")]