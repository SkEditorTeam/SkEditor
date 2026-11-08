using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Xaml.Behaviors.SourceGenerators;

[assembly: GenerateEventCommand(typeof(FATabViewItem), "CloseRequested")]
[assembly: GenerateEventCommand(typeof(ComboBox), "SelectionChanged")]
[assembly: GenerateEventCommand(typeof(TextBox), "PastingFromClipboard")]
[assembly: GenerateEventCommand(typeof(Button), "DoubleTapped")]
[assembly: GenerateEventCommand(typeof(Button), "PointerPressed")]