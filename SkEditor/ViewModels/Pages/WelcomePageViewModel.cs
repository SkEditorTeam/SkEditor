using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Projects;
using SkEditor.Views.Windows.Settings;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.ViewModels.Pages;

public partial class WelcomePageViewModel : ObservableObject
{
    public List<WelcomeScreenSection> WelcomeSections { get; } = [];

    [ObservableProperty]
    private ObservableCollection<WelcomeScreenRecentItem> _recentItems = [];

    public string Version { get; } = $"v{SkEditorAPI.Core.GetInformationalVersion()}";

    public WelcomePageViewModel()
    {
        WelcomeScreenSection gettingStarted = new(
            Translation.GetObservable("WelcomePageGettingStarted"),
            [
                new WelcomeScreenElement(Translation.GetObservable("WelcomePageGettingStartedNewFile"),
                    new RelayCommand(FileHandler.NewFile),
                    CreateSymbolIcon(Symbol.DocumentAdd)),
                new WelcomeScreenElement(Translation.GetObservable("WelcomePageGettingStartedOpenFile"),
                    new AsyncRelayCommand(FileHandler.OpenFile), CreateSymbolIcon(Symbol.DocumentSearch)),
                new WelcomeScreenElement(Translation.GetObservable("WelcomePageGettingStartedOpenFolder"),
                    new AsyncRelayCommand(async () => await ProjectOpener.OpenProject()),
                    CreateSymbolIcon(Symbol.FolderOpen)),
                new WelcomeScreenElement(Translation.GetObservable("WelcomePageGettingStartedSettings"),
                    new AsyncRelayCommand(OpenSettings),
                    CreateSymbolIcon(Symbol.Settings))
            ]
        );

        WelcomeScreenSection help = new(Translation.GetObservable("WelcomePageUsefulLinks"),
        [
            new WelcomeScreenElement(Translation.GetObservable("WelcomePageUsefulLinksDiscordServer"),
                new RelayCommand(() =>
                    SkEditorAPI.Core.OpenLink("https://discord.skeditor.dev")), CreatePathIconSource("DiscordIcon")),

            new WelcomeScreenElement(Translation.GetObservable("WelcomePageUsefulLinksGitHub"), new RelayCommand(() =>
                    SkEditorAPI.Core.OpenLink("https://github.com/SkEditorTeam/SkEditor")),
                CreatePathIconSource("GitHubIcon")),

            new WelcomeScreenElement(Translation.GetObservable("WelcomePageUsefulLinksDocumentation"),
                new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://docs.skeditor.dev")),
                CreateSymbolIcon(Symbol.Book)),

            new WelcomeScreenElement(Translation.GetObservable("WelcomePageUsefulLinksSupportUs"),
                new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://ko-fi.com/notro")),
                CreateSymbolIcon(Symbol.Heart))
        ]);

        WelcomeSections.Add(gettingStarted);
        WelcomeSections.Add(help);

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
         _recentItems.AddRange(recentItems);
    }

    private static SymbolIconSource CreateSymbolIcon(Symbol symbol)
    {
        return new SymbolIconSource
        {
            Symbol = symbol
        };
    }

    private static IconSource? CreatePathIconSource(string name)
    {
        return SkEditorAPI.Core.GetApplicationResource(name) as PathIconSource;
    }

    private static async Task OpenSettings()
    {
        await new SettingsWindow().ShowDialogOnMainWindow();
    }

    public record WelcomeScreenSection(TranslatableString Title, List<WelcomeScreenElement> Entries);

    public record WelcomeScreenElement(TranslatableString Name, IRelayCommand Command, IconSource? Icon);

    public record WelcomeScreenRecentItem(string Name, string Path, bool IsFolder, DateTime LastOpened)
    {
        public Symbol IconSymbol => IsFolder ? Symbol.Folder : Symbol.Document;
        public string DisplayPath => IsFolder ? Path : System.IO.Path.GetDirectoryName(Path) ?? string.Empty;
        public TranslatableString PrettyLastOpened => ToPrettyDate(LastOpened);

        public static TranslatableString ToPrettyDate(DateTime date)
        {
            var appCulture = CultureInfo.CurrentCulture;
            var span = DateTime.Now - date;

            switch (span.TotalDays)
            {
                case < 0:
                    return Translation.GetObservable("WelcomePageRecentInTheFuture");
                case >= 365 * 50:
                    return Translation.GetObservable("WelcomePageRecentTimeTraveler");
            }

            if (span.TotalMinutes < 1)
                return Translation.GetObservable("WelcomePageRecentJustNow");

            if (span.TotalHours < 1)
            {
                int minutes = (int)span.TotalMinutes;
                return Translation.GetObservable($"WelcomePageRecentMinutes{GetPluralKey(minutes)}",
                    minutes.ToString());
            }

            if (!(span.TotalDays < 1))
            {
                return date.Date == DateTime.Today.AddDays(-1)
                    ? Translation.GetObservable("WelcomePageRecentYesterdayAt", date.ToString("t", appCulture))
                    : new TranslatableString(date.ToString("g", appCulture));
            }

            int hours = (int)span.TotalHours;
            return Translation.GetObservable($"WelcomePageRecentHours{GetPluralKey(hours)}", hours.ToString());
        }

        public static string GetPluralKey(int n)
        {
            if (n == 1) return "One";

            if (n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20))
            {
                return "Few";
            }

            return "Many";
        }
    }
}