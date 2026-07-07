using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Models.Messages;
using SkEditor.Models.Settings;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Providers.Sections;

namespace SkEditor.ViewModels.Tabs;

public partial class SettingsTabViewModel : TabViewModelBase, IRecipient<NavigateToSettingsSectionMessage>
{
    private readonly IConfigService _configs;
    private readonly IMessenger _messenger;

    public SettingsTabViewModel(IConfigService configs, IEnumerable<ISettingsSectionProvider> sectionProviders,
        IMessenger messenger) : base(
        TranslationKey.SettingsHomeTitle, Symbol.Settings)
    {
        _configs = configs;
        _messenger = messenger;
        SettingsCategories = [];

        foreach (ISettingsSectionProvider settingsSectionProvider in sectionProviders)
        {
            SettingsCategories.Add(settingsSectionProvider.GetSection());
        }

        CurrentSection = SettingsCategories[0];

        _messenger.Register(this);
    }

    public override IRelayCommand CloseCommand => CloseTabCommand;

    public override bool IsCacheable => true;

    [ObservableProperty]
    public partial SettingSection? CurrentSection { get; set; }

    public List<SettingSection> SettingsCategories { get; }

    [RelayCommand]
    private void CloseTab()
    {
        _ = _configs.SaveAllConfigsAsync();
        _messenger.UnregisterAll(this);
        RequestClose();
    }

    public void Receive(NavigateToSettingsSectionMessage message)
    {
        CurrentSection = SettingsCategories.FirstOrDefault(s => s.Id == message.SectionId) ?? CurrentSection;
    }
}