using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using SkEditor.Configuration.Messages;
using SkEditor.Configuration.Models;
using SkEditor.Configuration.Providers.Sections;
using SkEditor.Core.Localization;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Configuration.UI;

public partial class SettingsTabViewModel : TabViewModelBase, IRecipient<NavigateToSettingsSectionMessage>, IClosableTab
{
    private readonly IConfigService _configs;
    private readonly IMessenger _messenger;
    private readonly Dictionary<string, SettingSection> _parentMap = [];

    private readonly Dictionary<string, SettingSection> _sectionsById;

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
        _sectionsById = [];

        var providerData = sectionProviders
            .Select(p => new { Provider = p, Section = p.GetSection() })
            .ToList();

        foreach (var item in providerData)
        {
            RegisterSection(item.Section, null);
        }

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

        SettingsCategories.Sort((a, b) => a.Order.CompareTo(b.Order));

        foreach (SettingSection section in _sectionsById.Values)
        {
            if (section.Subsections.Count <= 1)
            {
                continue;
            }

            List<SettingSection> sorted = [.. section.Subsections.OrderBy(s => s.Order)];
            section.Subsections.Clear();
            foreach (SettingSection s in sorted)
            {
                section.Subsections.Add(s);
            }
        }

        if (SettingsCategories.Count > 0)
        {
            CurrentSection = SettingsCategories[0];
        }

        _messenger.Register(this);
        return;

        void RegisterSection(SettingSection section, SettingSection? parent)
        {
            _sectionsById[section.Id] = section;
            if (parent is not null)
            {
                _parentMap[section.Id] = parent;
            }

            foreach (SettingSection sub in section.Subsections)
            {
                RegisterSection(sub, section);
            }
        }
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

    public void OnClosed()
    {
        _ = _configs.SaveAllConfigsAsync();
        _messenger.UnregisterAll(this);
    }
}