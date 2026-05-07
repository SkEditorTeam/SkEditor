using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Tabs;
using SkEditor.Models.Welcome;
using SkEditor.Utilities;
using SkEditor.Utilities.Icons;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Types;

namespace SkEditor.Services;

public class WelcomeService : IWelcomeService
{
    private readonly IFileService _files;

    private readonly SortedObservableCollection<WelcomeScreenRecentItem> _recentItems;
    private readonly Func<SettingsTab> _settingsTabFactory;
    private readonly ITabService _tabs;

    private DispatcherTimer _timeUpdater;

    public WelcomeService(IFileService fileService, ITabService tabService, Func<SettingsTab> settingsTabFactory)
    {
        _files = fileService;
        _tabs = tabService;
        _settingsTabFactory = settingsTabFactory;

        _recentItems = new SortedObservableCollection<WelcomeScreenRecentItem>(
            Comparer<WelcomeScreenRecentItem>.Create((a, b) => b.LastOpened.CompareTo(a.LastOpened))
        );
        RecentItems = new ReadOnlyObservableCollection<WelcomeScreenRecentItem>(_recentItems);

        BuildSections();
        BuildRecentItems();

        StartDateTimer();
    }

    public ReadOnlyObservableCollection<WelcomeScreenRecentItem> RecentItems { get; }

    public ObservableCollection<WelcomeScreenSection> WelcomeSections { get; } = [];

    private void BuildRecentItems()
    {
        List<WelcomeScreenRecentItem> recentItems =
        [
            new("core.sk", @"C:\Users\NotroDev\Documents\Server\plugins\Skript\scripts\core.sk",
                false, DateTime.Now.AddMinutes(-12)),
            new("SurvivalCore", @"D:\Projects\Minecraft\SurvivalCore", true,
                DateTime.Now.AddHours(-3)),
            new("economy.sk", @"C:\Users\Admin\Desktop\OLD_PROJECTS\backup_2025\economy.sk",
                false, DateTime.Now.AddDays(-1).AddHours(2)),
            new("scripts", @"Z:\NetworkDrive\Public\ServerFiles\Skript\scripts", true,
                DateTime.Now.AddDays(-4)),
            new("very_long_name_of_a_script_that_can_destroy_your_beautiful_ui_test.sk",
                @"C:\Temp\very_long_name_of_a_script_that_can_destroy_your_beautiful_ui_test.sk", false,
                DateTime.Now.AddDays(-10)),
            new("MiniGames", @"E:\Work\Java\SpigotServer\plugins\Skript\scripts\MiniGames",
                true, DateTime.Now.AddMinutes(-45)),
            new("adminchat_final_version.sk",
                @"C:\Users\NotroDev\Desktop\syf\adminchat_final_version.sk", false,
                new DateTime(2025, 03, 24, 10, 30, 0)),
            new("ChatManager", @"C:\Users\NotroDev\AppData\Roaming\SkEditor\temp\ChatManager",
                true, DateTime.Now.AddSeconds(-30))
        ];

        _recentItems.AddRange(recentItems);
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
                    new AsyncRelayCommand(() => Task.CompletedTask),
                    Icons.CreateSymbolIcon(Symbol.FolderOpen)).Disable(),
                WelcomeScreenElement.Create(TranslationKey.WelcomePageGettingStartedSettings,
                    new RelayCommand(() => _tabs.OpenUniqueTab(_settingsTabFactory)),
                    Icons.CreateSymbolIcon(Symbol.Settings))
            ]
        );

        WelcomeScreenSection help = new(new TranslatableString(TranslationKey.WelcomePageUsefulLinks),
        [
            WelcomeScreenElement.Create(TranslationKey.WelcomePageUsefulLinksDiscordServer,
                new RelayCommand(() =>
                    LinkOpener.OpenLink("https://discord.skeditor.dev")),
                Icons.GetPathIconSource("DiscordIcon")),

            WelcomeScreenElement.Create(TranslationKey.WelcomePageUsefulLinksGitHub,
                new RelayCommand(() =>
                    LinkOpener.OpenLink("https://github.com/SkEditorTeam/SkEditor")),
                Icons.GetPathIconSource("GitHubIcon")),

            WelcomeScreenElement.Create(TranslationKey.WelcomePageUsefulLinksDocumentation,
                new RelayCommand(() => LinkOpener.OpenLink("https://docs.skeditor.dev")),
                Icons.CreateSymbolIcon(Symbol.Book)),

            WelcomeScreenElement.Create(TranslationKey.WelcomePageUsefulLinksSupportUs,
                new RelayCommand(() => LinkOpener.OpenLink("https://ko-fi.com/notro")),
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
            Interval = TimeSpan.FromMinutes(1)
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