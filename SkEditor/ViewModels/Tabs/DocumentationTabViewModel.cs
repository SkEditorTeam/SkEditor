using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Tabs;

public partial class DocumentationTabViewModel(IConfigService configs)
    : TabViewModelBase(TranslationKey.SettingsHomeTitle, Symbol.Settings)
{
    private readonly IConfigService _configs = configs;

    public override IRelayCommand CloseCommand => CloseTabCommand;

    public override bool IsCacheable => true;

    [RelayCommand]
    private void CloseTab()
    {
        RequestClose();
    }
}