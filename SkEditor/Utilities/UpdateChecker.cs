using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Octokit;
using SkEditor.API;
using SkEditor.Utilities.Extensions;
using SkEditor.Views;
using Application = Avalonia.Application;
using FileMode = System.IO.FileMode;

namespace SkEditor.Utilities;

public static class UpdateChecker
{
    private const long RepoId = 679628726;
    public static readonly int Major = Assembly.GetExecutingAssembly().GetName().Version?.Major ?? 0;
    public static readonly int Minor = Assembly.GetExecutingAssembly().GetName().Version?.Minor ?? 0;
    public static readonly int Build = Assembly.GetExecutingAssembly().GetName().Version?.Build ?? 0;
    private static readonly GitHubClient GitHubClient = new(new ProductHeaderValue("SkEditor"));

    private static readonly string TempInstallerFile = Path.Combine(Path.GetTempPath(), "SkEditorInstaller.msi");

    public static async Task Check()
    {
        try
        {
            IReadOnlyList<Release> releases = await GitHubClient.Repository.Release.GetAll(RepoId);
            Release? release = releases.FirstOrDefault(r => !r.Prerelease);
            if (release is null)
            {
                return;
            }

            (int, int, int) version = GetVersion(release.TagName);
            if (!IsNewerVersion(version))
            {
                return;
            }

            ContentDialogResult result = await SkEditorAPI.Windows.ShowDialog(
                Translation.Get("UpdateAvailable"),
                Translation.Get("UpdateAvailableMessage"),
                Symbol.ImportantFilled,
                primaryButtonText: "Yes",
                cancelButtonText: "No"
            );

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            if (!OperatingSystem.IsWindows())
            {
                await SkEditorAPI.Windows.ShowError("Automatic updates are only available on Windows for now.");
            }

            ReleaseAsset? msi = release.Assets.FirstOrDefault(asset => asset.Name.Equals("SkEditorInstaller.msi"));
            if (msi is null)
            {
                await SkEditorAPI.Windows.ShowError(Translation.Get("UpdateFailed"));
                return;
            }

            await DownloadMsi(msi.BrowserDownloadUrl);
        }
        catch
        {
            // ignored
        }
    }

    private static async Task DownloadMsi(string url)
    {
        MainWindow? mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow is null) return;
        
        TaskDialog td = CreateTaskDialog(mainWindow, url);
        object? result = await td.ShowAsync();

        TaskDialogStandardResult standardResult = (TaskDialogStandardResult)result;
        if (standardResult == TaskDialogStandardResult.Cancel)
        {
            await SkEditorAPI.Windows.ShowError(Translation.Get("UpdateFailed"));
        }
    }

    private static TaskDialog CreateTaskDialog(Visual visual, string url)
    {
        TaskDialog td = new()
        {
            Title = Translation.Get("DownloadingUpdateTitle"),
            ShowProgressBar = true,
            IconSource = new SymbolIconSource { Symbol = Symbol.Download },
            SubHeader = Translation.Get("Downloading")
        };

        td.Opened += async (_, _) => await DownloadUpdate(td, url);

        td.XamlRoot = visual;
        return td;
    }

    private static async Task DownloadUpdate(TaskDialog td, string url)
    {
        const TaskDialogProgressState state = TaskDialogProgressState.Normal;
        td.SetProgressBarState(0, state);

        try
        {
            using (HttpClient client = new())
            {
                Progress<float> progress = new();
                progress.ProgressChanged += (_, sender) => td.SetProgressBarState(sender, state);

                await using FileStream file = new(TempInstallerFile, FileMode.Create, FileAccess.Write, FileShare.None);
                await client.DownloadDataAsync(url, file, progress);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = TempInstallerFile,
                UseShellExecute = true
            });

            (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
        }
        catch (Exception e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                td.Hide(TaskDialogStandardResult.Cancel);
                SkEditorAPI.Windows.ShowError(Translation.Get("UpdateFailed") + "\n" + e.Message);
            });
        }
    }

    private static (int, int, int) GetVersion(string tagName)
    {
        tagName = tagName.TrimStart('v');
        string[] versionParts = tagName.Split('.');
        return (int.Parse(versionParts[0]), int.Parse(versionParts[1]), int.Parse(versionParts[2]));
    }

    private static bool IsNewerVersion((int, int, int) version)
    {
        return version.Item1 > Major ||
               (version.Item1 == Major && version.Item2 > Minor) ||
               (version.Item1 == Major && version.Item2 == Minor && version.Item3 > Build);
    }
}