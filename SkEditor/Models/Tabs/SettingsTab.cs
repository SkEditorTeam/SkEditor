using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Pages;

namespace SkEditor.Models.Tabs;

public partial class SettingsTab(SettingsPageViewModel pageViewModel, IConfigService configs)
    : Tab(TranslationKey.SettingsHomeTitle, Symbol.Settings)
{
    public SettingsPageViewModel PageViewModel { get; } = pageViewModel;

    public override IRelayCommand CloseCommand => CloseTabCommand;

    public override bool IsCacheable => true;

    [RelayCommand]
    private void CloseTab()
    {
        _ = configs.SaveAllConfigsAsync();
        RequestClose();
    }
}