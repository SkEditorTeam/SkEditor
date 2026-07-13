using System.Collections.ObjectModel;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R3;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Analyzer;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.ViewModels.Controls.StatusBar.Panes;

public partial class ErrorListPaneViewModel : ObservableObject, IDrawerPaneViewModel
{
    private readonly ITabService _tabs;

    public ErrorListPaneViewModel(ITabService tabs, ExperimentsConfig experiments)
    {
        _tabs = tabs;

        IsVisible = experiments
            .ObservePropertyChanged(x => x.AnalyzerEnabled)
            .ToBindableReactiveProperty();
    }

    public BindableReactiveProperty<bool> IsAnalyzerLoaded { get; set; } = new(false);

    public ObservableCollection<AnalyzerError> Errors { get; set; } = [];

    public string Id => "ErrorList";
    public TranslatableString Name => "Errors".ToTranslatable();
    public BindableReactiveProperty<bool> IsVisible { get; }

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