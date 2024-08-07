using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Octokit;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
    private static readonly string _tempInstallerFile = Path.Combine(Path.GetTempPath(), OperatingSystem.IsWindows() ? "SkEditorInstaller.msi" : "SkEditorForLinux.zip");
    private static readonly string _tempUnpackDirLinux = Path.Combine(Path.GetTempPath(), "SkEditorUpdate");

    public static async void Check()
    {
        try
        {
            var releases = await _gitHubClient.Repository.Release.GetAll(RepoId);
            var release = releases.FirstOrDefault(r => !r.Prerelease);
            var version = GetVersion(release.TagName);
            if (!IsNewerVersion(version)) return;

            var result = await SkEditorAPI.Windows.ShowDialog(
                Translation.Get("UpdateAvailable"),
                Translation.Get("UpdateAvailableMessage"),
                Symbol.ImportantFilled,
                primaryButtonText: "Yes",
                cancelButtonText: "No"
            );

            if (result != ContentDialogResult.Primary) return;

            if (OperatingSystem.IsWindows())
                await DownloadAndUpdate(release, "SkEditorInstaller.msi", version);
            else if (OperatingSystem.IsLinux())
                await DownloadAndUpdate(release, "SkEditorForLinux.zip", version);
            else
                await SkEditorAPI.Windows.ShowError("Automatic updates are only available on Windows and Linux for now.");
        }
        catch (Exception ex)
        {
            await SkEditorAPI.Windows.ShowError($"Update check failed: {ex.Message}");
        }
    }

    private static async Task DownloadAndUpdate(Release release, string assetName, (int, int, int) version)
    {
        var asset = release.Assets.FirstOrDefault(a => a.Name.Equals(assetName));
        if (asset == null)
        {
            await SkEditorAPI.Windows.ShowError(Translation.Get("UpdateFailed"));
            return;
        }

        var td = CreateTaskDialog(SkEditorAPI.Windows.GetMainWindow(), asset.BrowserDownloadUrl);
        var result = await td.ShowAsync();
        if ((TaskDialogStandardResult)result == TaskDialogStandardResult.Cancel)
        {
            await SkEditorAPI.Windows.ShowError(Translation.Get("UpdateFailed"));
            return;
        }

        if (OperatingSystem.IsWindows())
            Process.Start(new ProcessStartInfo { FileName = _tempInstallerFile, UseShellExecute = true, Verb = "runas" });
        else
            await UnpackAndUpdateLinux(version);

        (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
    }

    private static TaskDialog CreateTaskDialog(Visual visual, string url)
    {
        var td = new TaskDialog
        {
            Title = Translation.Get("DownloadingUpdateTitle"),
            ShowProgressBar = true,
            IconSource = new SymbolIconSource { Symbol = Symbol.Download },
            SubHeader = Translation.Get("Downloading"),
            XamlRoot = visual
        };
        td.Opened += async (s, e) => await DownloadUpdate(td, url);
        return td;
    }

    private static async Task DownloadUpdate(TaskDialog td, string url)
    {
        using var client = new HttpClient();
        using var file = new FileStream(_tempInstallerFile, FileMode.Create, FileAccess.Write, FileShare.None);
        await client.DownloadDataAsync(url, file, new Progress<float>((p) => td.SetProgressBarState(p, TaskDialogProgressState.Normal)));
        Dispatcher.UIThread.Post(() => { td.Hide(TaskDialogStandardResult.Ok); });
    }

    private static async Task UnpackAndUpdateLinux((int, int, int) version)
    {
        if (Directory.Exists(_tempUnpackDirLinux))
            Directory.Delete(_tempUnpackDirLinux, true);
        Directory.CreateDirectory(_tempUnpackDirLinux);

        ZipFile.ExtractToDirectory(_tempInstallerFile, _tempUnpackDirLinux);

        foreach (var file in Directory.GetFiles(_tempUnpackDirLinux, "*", SearchOption.AllDirectories))
        {
            var destinationPath = Path.Combine("/opt/SkEditor", Path.GetRelativePath(_tempUnpackDirLinux, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
            File.Copy(file, destinationPath, true);
        }

        await SkEditorAPI.Windows.ShowError("Update completed successfully. Please restart the application.");
    }

    private static (int, int, int) GetVersion(string tagName)
    {
        var versionParts = tagName.TrimStart('v').Split('.').Select(int.Parse).ToArray();
        return (versionParts[0], versionParts[1], versionParts[2]);
    }

    private static bool IsNewerVersion((int, int, int) version) =>
        version.Item1 > _major ||
        (version.Item1 == _major && version.Item2 > _minor) ||
        (version.Item1 == _major && version.Item2 == _minor && version.Item3 > _build);
}
