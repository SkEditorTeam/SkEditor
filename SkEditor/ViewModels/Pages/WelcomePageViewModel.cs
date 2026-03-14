using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.API;
using SkEditor.Models.Welcome;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Icons;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Projects;
using SkEditor.Views.Windows.Settings;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.ViewModels.Pages;

public partial class WelcomePageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<WelcomeScreenRecentItem> _recentItems = [];

    private DispatcherTimer _timeUpdater;

    public WelcomePageViewModel()
    {
        BuildSections();
        BuildRecentItems();

        StartDateTimer();
    }

    public List<WelcomeScreenSection> WelcomeSections { get; } = [];

    public string Version { get; } = $"v{SkEditorAPI.Core.GetInformationalVersion()}";

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
        recentItems.Sort((a, b) => b.LastOpened.CompareTo(a.LastOpened));
        RecentItems.AddRange(recentItems);
    }

    private void BuildSections()
    {
        WelcomeScreenSection gettingStarted = new(
            Translation.GetObservable(TranslationKey.WelcomePageGettingStarted),
            [
                new WelcomeScreenElement(Translation.GetObservable(TranslationKey.WelcomePageGettingStartedNewFile),
                    new RelayCommand(FileHandler.NewFile),
                    Icons.CreateSymbolIcon(Symbol.DocumentAdd)),
                new WelcomeScreenElement(Translation.GetObservable(TranslationKey.WelcomePageGettingStartedOpenFile),
                    new AsyncRelayCommand(FileHandler.OpenFile), Icons.CreateSymbolIcon(Symbol.DocumentSearch)),
                new WelcomeScreenElement(Translation.GetObservable(TranslationKey.WelcomePageGettingStartedOpenFolder),
                    new AsyncRelayCommand(async () => await ProjectOpener.OpenProject()),
                    Icons.CreateSymbolIcon(Symbol.FolderOpen)),
                new WelcomeScreenElement(Translation.GetObservable(TranslationKey.WelcomePageGettingStartedSettings),
                    new AsyncRelayCommand(OpenSettings),
                    Icons.CreateSymbolIcon(Symbol.Settings))
            ]
        );

        WelcomeScreenSection help = new(Translation.GetObservable(TranslationKey.WelcomePageUsefulLinks),
        [
            new WelcomeScreenElement(Translation.GetObservable(TranslationKey.WelcomePageUsefulLinksDiscordServer),
                new RelayCommand(() =>
                    SkEditorAPI.Core.OpenLink("https://discord.skeditor.dev")),
                Icons.CreatePathIconSource("DiscordIcon")),

            new WelcomeScreenElement(Translation.GetObservable(TranslationKey.WelcomePageUsefulLinksGitHub),
                new RelayCommand(() =>
                    SkEditorAPI.Core.OpenLink("https://github.com/SkEditorTeam/SkEditor")),
                Icons.CreatePathIconSource("GitHubIcon")),

            new WelcomeScreenElement(Translation.GetObservable(TranslationKey.WelcomePageUsefulLinksDocumentation),
                new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://docs.skeditor.dev")),
                Icons.CreateSymbolIcon(Symbol.Book)),

            new WelcomeScreenElement(Translation.GetObservable(TranslationKey.WelcomePageUsefulLinksSupportUs),
                new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://ko-fi.com/notro")),
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

    private static async Task OpenSettings()
    {
        await App.Services.GetRequiredService<SettingsWindow>().ShowDialogOnMainWindow();
    }
}