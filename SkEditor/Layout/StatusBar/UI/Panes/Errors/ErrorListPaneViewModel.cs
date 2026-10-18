using System.Collections.ObjectModel;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R3;
using SkEditor.Analyzing.Models;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Localization;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Layout.StatusBar.UI.Abstractions;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Extensions;

namespace SkEditor.Layout.StatusBar.UI.Panes.Errors;

public partial class ErrorListPaneViewModel : ObservableObject, IDrawerPaneViewModel
{
    private readonly ITabService _tabs;

    public ObservableCollection<AnalyzerError> Errors { get; set; } = [];

    public string Id => "ErrorList";

    public BindableReactiveProperty<bool> IsAnalyzerLoaded { get; set; } = new(false);
    public BindableReactiveProperty<bool> IsFileAnalyzable { get; set; } = new(false);
    public BindableReactiveProperty<bool> IsVisible { get; }
    public TranslatableString Name => TranslationKey.BottomDrawerErrorListTitle;

    public ErrorListPaneViewModel(ITabService tabs, ExperimentsConfig experiments)
    {
        _tabs = tabs;

        IsVisible = experiments
            .ObservePropertyChanged(x => x.AnalyzerEnabled)
            .ToBindableReactiveProperty();

        _tabs.ObserveSelectedTab()
            .Subscribe(tab => { IsFileAnalyzable.Value = tab is FileTabViewModel { IsAnalyzable: true }; });
    }

    [RelayCommand]
    public void ErrorClick(AnalyzerError error)
    {
        if (_tabs.SelectedTab is FileTabViewModel fileTab)
        {
            DocumentLine line = fileTab.Document.GetLineByNumber(error.Line);
            fileTab.RequestGoToLocation(new TextLocation(error.Line, line.EndOffset));
        }
    }
}