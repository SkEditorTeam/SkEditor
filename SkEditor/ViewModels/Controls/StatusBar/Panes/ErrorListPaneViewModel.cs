using System.Collections.ObjectModel;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Analyzer;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.ViewModels.Controls.StatusBar.Panes;

public partial class ErrorListPaneViewModel(INotificationService notifs, ITabService tabs)
    : ObservableObject, IDrawerPaneViewModel
{
    public TranslatableString Name => TranslatableString.FromRawString("Errors");

    public ObservableCollection<AnalyzerError> Errors { get; set; } = [];

    [RelayCommand]
    public void ErrorClick(AnalyzerError error)
    {
        if (tabs.SelectedTab is FileTabViewModel fileTab)
        {
            DocumentLine line = fileTab.Document.GetLineByNumber(error.Line);
            fileTab.RequestGoToLocation(new TextLocation(error.Line, line.EndOffset));
        }
    }
}