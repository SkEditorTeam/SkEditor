using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Models.Analyzer;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls.Panes;

public partial class ErrorListPaneViewModel(INotificationService notifs) : ObservableObject, IDrawerPaneViewModel
{
    public TranslatableString Name => TranslatableString.FromRawString("Errors");

    public List<AnalyzerError> Errors { get; set; } =
    [
        new(AnalyzerErrorType.Error,
            "Can't understand this condition/effect: set yaml value \"log\" from file \"spigot.yml\" to false",
            "login.sk", 54),
        new(AnalyzerErrorType.Error,
            "Can't understand this condition: 'file \"plugins/Skript/scripts/players/%the sender%.yml\" exists'",
            "login.sk", 67)
    ];

    [RelayCommand]
    public void ErrorClick()
    {
        notifs.Debug("you clicked the error");
    }
}