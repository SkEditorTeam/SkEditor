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

    private static readonly string _tempInstallerFileWindows = Path.Combine(Path.GetTempPath(), "SkEditorInstaller.msi");
    private static readonly string _tempInstallerFileLinux = Path.Combine(Path.GetTempPath(), "SkEditorForLinux.zip");
    private static readonly string _tempUnpackDirLinux = Path.Combine(Path.GetTempPath(), "SkEditorUpdate");

    public static async void Check()
    {
        try
        {
            IReadOnlyList<Release> releases = await _gitHubClient.Repository.Release.GetAll(RepoId);
            Release release = releases.FirstOrDefault(r => !r.Prerelease);

            (int, int, int) version = GetVersion(release.TagName);
            if (!IsNewerVersion(version)) return;

            ContentDialogResult result = await SkEditorAPI.Windows.ShowDialog(
                Translation.Get("UpdateAvailable"),
                Translation.Get("UpdateAvailableMessage"),
                Symbol.ImportantFilled,
                primaryButtonText: "Yes",
                cancelButtonText: "No"
            );

            if (result != ContentDialogResult.Primary) return;

            if (OperatingSystem.IsWindows())
            {
                await UpdateWindows(release);
            }
            else if (OperatingSystem.IsLinux())
            {
                await UpdateLinux(release, version);
            }
            else
            {
                await SkEditorAPI.Windows.ShowError("Automatic updates are only available on Windows and Linux for now.");
            }
        }
        catch (Exception ex)
        {
            await SkEditorAPI.Windows.ShowError($"Update check failed: {ex.Message}");
        }
    }

    private static async Task UpdateWindows(Release release)
    {
        ReleaseAsset msi = release.Assets.FirstOrDefault(asset => asset.Name.Equals("SkEditorInstaller.msi"));
        if (msi is null)
        {
            await SkEditorAPI.Windows.ShowError(Translation.Get("UpdateFailed"));
            return;
        }
        await DownloadFile(msi.BrowserDownloadUrl, _tempInstallerFileWindows, true);
    }

    private static async Task UpdateLinux(Release release, (int, int, int) version)
    {
        ReleaseAsset zip = release.Assets.FirstOrDefault(asset => asset.Name.Equals("SkEditorForLinux.zip"));
        if (zip is null)
        {
            await SkEditorAPI.Windows.ShowError(Translation.Get("UpdateFailed"));
            return;
        }
        await DownloadFile(zip.BrowserDownloadUrl, _tempInstallerFileLinux, false);
        await UnpackAndUpdateLinux(version);
    }

    private static async Task DownloadFile(string url, string filePath, bool isWindows)
    {
        TaskDialog td = CreateTaskDialog(SkEditorAPI.Windows.GetMainWindow(), url);
        var result = await td.ShowAsync();

        TaskDialogStandardResult standardResult = (TaskDialogStandardResult)result;
        if (standardResult == TaskDialogStandardResult.Cancel)
        {
            await SkEditorAPI.Windows.ShowError(Translation.Get("UpdateFailed"));
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

                using var file = new FileStream(url.Contains("msi") ? _tempInstallerFileWindows : _tempInstallerFileLinux, FileMode.Create, FileAccess.Write, FileShare.None);
                await client.DownloadDataAsync(url, file, progress);
            }

            if (url.Contains("msi"))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _tempInstallerFileWindows,
                    UseShellExecute = true,
                    Verb = "runas"
                });

                (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
            }
            else
            {
                Dispatcher.UIThread.Post(() => { td.Hide(TaskDialogStandardResult.Ok); });
            }
        }
        catch
        {
            Dispatcher.UIThread.Post(() => { td.Hide(TaskDialogStandardResult.Cancel); });
        }
    }

    private static async Task UnpackAndUpdateLinux((int, int, int) version)
    {
        try
        {
            if (Directory.Exists(_tempUnpackDirLinux))
            {
                Directory.Delete(_tempUnpackDirLinux, true);
            }
            Directory.CreateDirectory(_tempUnpackDirLinux);

            using (FileStream fs = new FileStream(_tempInstallerFileLinux, FileMode.Open))
            using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name)) // Skip directories
                    {
                        continue;
                    }

                    string destinationPath = Path.Combine("/opt/SkEditor", entry.FullName);
                    string destinationDirectory = Path.GetDirectoryName(destinationPath);
                    if (!Directory.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    entry.ExtractToFile(destinationPath, true);
                }
            }

            await SkEditorAPI.Windows.ShowError("Update completed successfully. Please restart the application.");
        }
        catch (Exception ex)
        {
            await SkEditorAPI.Windows.ShowError($"Update failed: {ex.Message}");
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
