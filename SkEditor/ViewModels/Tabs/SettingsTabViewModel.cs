using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
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
    private readonly Dictionary<string, SettingSection> _parentMap = [];

    private readonly Dictionary<string, SettingSection> _sectionsById;

    public override IRelayCommand CloseCommand => CloseTabCommand;

    [ObservableProperty]
    public partial SettingSection? CurrentSection { get; set; }

    public override bool IsCacheable => true;

    public List<SettingSection> SettingsCategories { get; }

    public SettingsTabViewModel(IConfigService configs, IEnumerable<ISettingsSectionProvider> sectionProviders,
        IMessenger messenger) : base(
        TranslationKey.SettingsHomeTitle, Symbol.Settings)
    {
        _configs = configs;
        _messenger = messenger;
        SettingsCategories = [];

        var providerData = sectionProviders
            .Select(p => new { Provider = p, Section = p.GetSection() })
            .ToList();

        _sectionsById = providerData.ToDictionary(x => x.Section.Id, x => x.Section);

        foreach (var item in providerData)
        {
            string? parentId = item.Provider.ParentId;

            if (string.IsNullOrEmpty(parentId))
            {
                SettingsCategories.Add(item.Section);
            }
            else if (_sectionsById.TryGetValue(parentId, out SettingSection? parentSection))
            {
                parentSection.Subsections.Add(item.Section);
                _parentMap[item.Section.Id] = parentSection;
            }
            else
            {
                SettingsCategories.Add(item.Section);
            }
        }

        if (SettingsCategories.Count > 0)
        {
            CurrentSection = SettingsCategories[0];
        }

        _messenger.Register(this);
    }

    public void Receive(NavigateToSettingsSectionMessage message)
    {
        if (!_sectionsById.TryGetValue(message.SectionId, out SettingSection? section))
        {
            return;
        }

        SettingSection current = section;
        while (_parentMap.TryGetValue(current.Id, out SettingSection? parent))
        {
            parent.IsExpanded.Value = true;
            current = parent;
        }

        Dispatcher.UIThread.InvokeAsync(() => { CurrentSection = section; }, DispatcherPriority.Background);
    }

    [RelayCommand]
    private void CloseTab()
    {
        _ = _configs.SaveAllConfigsAsync();
        _messenger.UnregisterAll(this);
        RequestClose();
    }
}