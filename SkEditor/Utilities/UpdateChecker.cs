using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Octokit;
using SkEditor.API;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Application = Avalonia.Application;
using FileMode = System.IO.FileMode;

namespace SkEditor.Utilities;
public static class UpdateChecker
{
    private static readonly int _major = Assembly.GetExecutingAssembly().GetName().Version.Major;
    private static readonly int _minor = Assembly.GetExecutingAssembly().GetName().Version.Minor;
    private static readonly int _build = Assembly.GetExecutingAssembly().GetName().Version.Build;

    private const long RepoId = 679628726;
    private static readonly GitHubClient _gitHubClient = new(new ProductHeaderValue("SkEditor"));

    private static readonly string _tempInstallerFile = Path.Combine(Path.GetTempPath(), "SkEditorInstaller.msi");

    public static async void Check()
    {
        try
        {
            Release release = await _gitHubClient.Repository.Release.GetLatest(RepoId);
            (int, int, int) version = GetVersion(release.TagName);
            if (!IsNewerVersion(version)) return;

            SymbolIconSource icon = new() { Symbol = Symbol.ImportantFilled };
            ContentDialogResult result = await ApiVault.Get().ShowMessageWithIcon(
                Translation.Get("UpdateAvailable"),
                Translation.Get("UpdateAvailableMessage"),
                icon,
                primaryButtonContent: Translation.Get("Yes"),
                closeButtonContent: Translation.Get("No")
            );

            if (result != ContentDialogResult.Primary) return;

            if (!OperatingSystem.IsWindows())
            {
                ApiVault.Get().ShowMessage(Translation.Get("Error"), "Automatic updates are only available on Windows for now.\nContact Notro for an updated version.");
            }

            ReleaseAsset msi = release.Assets.FirstOrDefault(asset => asset.Name.Equals("SkEditorInstaller.msi"));
            if (msi is null)
            {
                ApiVault.Get().ShowMessage(Translation.Get("Error"), Translation.Get("UpdateFailed"));
                return;
            }
            DownloadMsi(msi.BrowserDownloadUrl);
        }
        catch { }
    }

    private async static void DownloadMsi(string url)
    {
        TaskDialog td = CreateTaskDialog(SkEditorAPI.Windows.GetMainWindow(), url);
        var result = await td.ShowAsync();

        TaskDialogStandardResult standardResult = (TaskDialogStandardResult)result;
        if (standardResult == TaskDialogStandardResult.Cancel)
        {
            ApiVault.Get().ShowMessage(Translation.Get("Error"), Translation.Get("UpdateFailed"));
        }
    }

    private static TaskDialog CreateTaskDialog(Visual visual, string url)
    {
        var td = new TaskDialog
        {
            Title = Translation.Get("DownloadingUpdateTitle"),
            ShowProgressBar = true,
            IconSource = new SymbolIconSource { Symbol = Symbol.Download },
            SubHeader = Translation.Get("Downloading"),
        };

        td.Opened += async (s, e) => await DownloadUpdate(td, url);

        td.XamlRoot = visual;
        return td;
    }

    private static async Task DownloadUpdate(TaskDialog td, string url)
    {
        TaskDialogProgressState state = TaskDialogProgressState.Normal;
        td.SetProgressBarState(0, state);

        try
        {
            using (HttpClient client = new())
            {
                var progress = new Progress<float>();
                progress.ProgressChanged += (e, sender) => td.SetProgressBarState(sender, state);

                using var file = new FileStream(_tempInstallerFile, FileMode.Create, FileAccess.Write, FileShare.None);
                await client.DownloadDataAsync(url, file, progress);
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = "msiexec",
                Arguments = $"/i \"{_tempInstallerFile}\" /quiet",
                UseShellExecute = true,
                Verb = "runas"
            };

            Process.Start(startInfo);

            (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
        }
        catch
        {
            Dispatcher.UIThread.Post(() => { td.Hide(TaskDialogStandardResult.Cancel); });
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
        return version.Item1 > _major ||
               (version.Item1 == _major && version.Item2 > _minor) ||
               (version.Item1 == _major && version.Item2 == _minor && version.Item3 > _build);
    }
}
