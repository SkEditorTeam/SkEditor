using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Pages;

namespace SkEditor.Models.Tabs;

public partial class SettingsTab(SettingsPageViewModel pageViewModel)
    : Tab(TranslationKey.SettingsHomeTitle, Symbol.Settings)
{
    public SettingsPageViewModel PageViewModel { get; } = pageViewModel;

    public override IRelayCommand CloseCommand => CloseTabCommand;

    [RelayCommand]
    private void CloseTab()
    {
        RequestClose();
    }
}