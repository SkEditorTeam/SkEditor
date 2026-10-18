using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Messages;
using SkEditor.Models.Welcome;
using SkEditor.Utilities.Icons;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Services;

public class WelcomeService : IWelcomeService
{
    private readonly IFileService _files;
    private readonly ILinkService _links;

    private readonly Func<SettingsTabViewModel> _settingsTabFactory;
    private readonly ITabService _tabs;

    private DispatcherTimer _timeUpdater;

    public ReadOnlyObservableCollection<WelcomeScreenRecentItem> RecentItems { get; }

    public ObservableCollection<WelcomeScreenSection> WelcomeSections { get; } = [];

    public WelcomeService(IFileService fileService, ITabService tabService, IRecentFileService recents,
        ILinkService links,
        Func<SettingsTabViewModel> settingsTabFactory)
    {
        _files = fileService;
        _links = links;
        _tabs = tabService;
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
                    new RelayCommand(() => _files.NewFile()), Icons.CreateSymbolIcon(Symbol.Document)),
                WelcomeScreenElement.Create(TranslationKey.WelcomePageGettingStartedOpenFile,
                    new AsyncRelayCommand(async () => await _files.OpenFileAsync()),
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