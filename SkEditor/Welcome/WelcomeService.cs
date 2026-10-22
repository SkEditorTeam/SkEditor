using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using SkEditor.Appearance.Icons;
using SkEditor.Configuration.UI;
using SkEditor.Core.Localization;
using SkEditor.Core.System;
using SkEditor.Welcome.Models;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Projects.Messages;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Welcome;

public class WelcomeService : IWelcomeService
{
    private readonly IFileTabService _fileTabs;
    private readonly ILinkService _links;

    private readonly Func<SettingsTabViewModel> _settingsTabFactory;
    private readonly ITabService _tabs;

    private DispatcherTimer _timeUpdater;

    public ReadOnlyObservableCollection<WelcomeScreenRecentItem> RecentItems { get; }

    public ObservableCollection<WelcomeScreenSection> WelcomeSections { get; } = [];

    public WelcomeService(IFileTabService fileTabs, ITabService tabs, IRecentFileService recents,
        ILinkService links,
        Func<SettingsTabViewModel> settingsTabFactory)
    {
        _fileTabs = fileTabs;
        _links = links;
        _tabs = tabs;
        _settingsTabFactory = settingsTabFactory;

        RecentItems = recents.RecentItems;
        _ = recents.LoadRecentItemsAsync();

        BuildSections();

        StartDateTimer();
    }

    private void BuildSections()
    {
        WelcomeScreenSection gettingStarted = new(
            new TranslatableString(TranslationKey.WelcomePageGettingStarted),
            [
                WelcomeScreenElement.Create(TranslationKey.WelcomePageGettingStartedNewFile,
                    new RelayCommand(() =>
                    {
                        ITab tab = _fileTabs.NewFile();
                        _tabs.OpenTab(tab);
                    }), Icons.CreateSymbolIcon(Symbol.Document)),
                WelcomeScreenElement.Create(TranslationKey.WelcomePageGettingStartedOpenFile,
                    new AsyncRelayCommand(async () => await _fileTabs.OpenFileAsync()),
                    Icons.CreateSymbolIcon(Symbol.DocumentSearch)),
                WelcomeScreenElement.Create(TranslationKey.WelcomePageGettingStartedOpenFolder,
                    new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenExplorerFolderMessage())),
                    Icons.CreateSymbolIcon(Symbol.FolderOpen)),
                WelcomeScreenElement.Create(TranslationKey.WelcomePageGettingStartedSettings,
                    new RelayCommand(() => _tabs.OpenUniqueTab(_settingsTabFactory)),
                    Icons.CreateSymbolIcon(Symbol.Settings))
            ]
        );

        WelcomeScreenSection help = new(new TranslatableString(TranslationKey.WelcomePageUsefulLinks),
        [
            WelcomeScreenElement.Create(TranslationKey.WelcomePageUsefulLinksDiscordServer,
                new RelayCommand(() =>
                    _links.OpenLink("https://discord.skeditor.dev")),
                Icons.GetPathIconSource("DiscordIcon")),

            WelcomeScreenElement.Create(TranslationKey.WelcomePageUsefulLinksGitHub,
                new RelayCommand(() =>
                    _links.OpenLink("https://github.com/SkEditorTeam/SkEditor")),
                Icons.GetPathIconSource("GitHubIcon")),

            WelcomeScreenElement.Create(TranslationKey.WelcomePageUsefulLinksDocumentation,
                new RelayCommand(() => _links.OpenLink("https://docs.skeditor.dev")),
                Icons.CreateSymbolIcon(Symbol.Book)),

            WelcomeScreenElement.Create(TranslationKey.WelcomePageUsefulLinksSupportUs,
                new RelayCommand(() => _links.OpenLink("https://ko-fi.com/notro")),
                Icons.CreateSymbolIcon(Symbol.Heart))
        ]);

        WelcomeSections.Add(gettingStarted);
        WelcomeSections.Add(help);
    }

    [MemberNotNull(nameof(_timeUpdater))]
    private void StartDateTimer()
    {
        _timeUpdater = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(15)
        };

        _timeUpdater.Tick += (_, _) =>
        {
            foreach (WelcomeScreenRecentItem item in RecentItems)
            {
                item.RefreshTime();
            }
        };

        _timeUpdater.Start();
    }
}