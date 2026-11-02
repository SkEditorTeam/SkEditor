using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentIcons.Common;
using Semver;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Common;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Core.System;
using SkEditor.Core.Updates.Models;
using SkEditor.Windowing;

namespace SkEditor.Core.Updates;

public class UpdateService(
    IHttpClientFactory factory,
    AppSettings appSettings,
    IAppInfo appInfo,
    ILogService logs,
    INotificationService notifs,
    IDialogService dialogs,
    IFileSystem fileSystem,
    ILinkService links,
    IWindowRegistry windowRegistry) : IUpdateService
{
    public async Task<ReleaseInfo?> CheckForUpdates()
    {
        const string url = "https://api.github.com/repos/SkEditorTeam/SkEditor/releases";

        _ = Task.Run(CleanupTemp);

        try
        {
            using HttpClient client = factory.CreateClient(HttpClients.SkEditor);

            await using Stream stream = await client.GetStreamAsync(url);

            bool includePreReleases = appSettings.IncludePreReleaseUpdates || appInfo.Version.IsPrerelease;

            List<GitHubRelease>? releases =
                JsonSerializer.Deserialize<List<GitHubRelease>>(stream,
                    GitHubReleaseJsonContext.Default.ListGitHubRelease);

            ReleaseInfo? targetRelease = releases?
                .Where(r => includePreReleases || !r.IsPrerelease)
                .Select(r => SemVersion.TryParse(r.TagName, SemVersionStyles.Any, out var v) 
                    ? new ReleaseInfo(r, v) 
                    : null)
                .OfType<ReleaseInfo>()
                .MaxBy(x => x.Version, SemVersion.PrecedenceComparer);

            bool newer = targetRelease?.Version.IsNewerThan(appInfo.Version) ?? false;
            return newer ? targetRelease : null;
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to check for updates");
            return null;
        }
    }

    public void ShowNotification(ReleaseInfo? info)
    {
        Notification notification;

        if (info is null)
        {
            notification = Notification.Create(TranslationKey.NoUpdatesAvailableTitle)
                .WithMessage(TranslationKey.NoUpdatesAvailableMessage)
                .WithType(NotificationType.Information);
        }
        else
        {
            notification = Notification.Create(TranslationKey.UpdateAvailableTitle)
                .WithMessage(TranslationKey.UpdateAvailableMessage)
                .WithType(NotificationType.Success);

            notification.WithOnClick(() => ShowUpdateDialog(info));
        }

        notification.WithDuration(TimeSpan.FromSeconds(10));

        notifs.Show(notification);
    }

    private async Task ShowUpdateDialog(ReleaseInfo info)
    {
        Dialog dialog = Dialog
            .Create(TranslationKey.UpdateAvailableTitle)
            .WithMessage(new TranslatableString(TranslationKey.UpdateAvailableMessageDownload, info.Version.ToString()))
            .WithIcon(Symbol.Sparkle)
            .WithButtons(
                new DialogButton(TranslationKey.UpdateAvailableDownloadButton, DialogResult.Yes, true),
                new DialogButton(TranslationKey.UpdateAvailableNotNowButton, DialogResult.No)
            );

        DialogResult result = await dialogs.ShowDialogAsync(dialog);
        if (result != StandardDialogResult.Yes)
        {
            return;
        }

        if (appInfo.IsInstalled)
        {
            await DownloadAndRunInstaller(info);
        }
        else
        {
            links.OpenLink(info.Release.HtmlUrl);
        }
    }

    private async Task DownloadAndRunInstaller(ReleaseInfo info)
    {
        GitHubAsset? installerAsset = info.Release.Assets.FirstOrDefault(a => a.Name.EndsWith("msi"));
        if (installerAsset is null)
        {
            logs.Error($"No installer asset found for release {info.Version}");
            return;
        }

        string tempFolder = Path.Combine(fileSystem.Path.GetTempPath(), "SkEditor");
        fileSystem.Directory.CreateDirectory(tempFolder);

        string filePath = Path.Combine(tempFolder, $"Installer-{info.Release.TagName}.msi");
        string tempPath = filePath + ".tmp";

        try
        {
            using HttpClient client = factory.CreateClient(HttpClients.SkEditor);
            await using (Stream downloadStream = await client.GetStreamAsync(installerAsset.DownloadUrl))
            await using (FileSystemStream fileStream = fileSystem.File.Create(tempPath))
            {
                await downloadStream.CopyToAsync(fileStream);
            }

            fileSystem.File.Move(tempPath, filePath, true);

            Notification notification = Notification.Create(TranslationKey.UpdateDownloadedTitle)
                .WithMessage(TranslationKey.UpdateDownloadedMessage)
                .WithType(NotificationType.Success)
                .WithDuration(TimeSpan.FromSeconds(20));

            notification.WithOnClick(async () =>
            {
                if (windowRegistry.Active is not { } mainWindow)
                {
                    return;
                }

                bool closed = await mainWindow.TryCloseAsync();
                if (closed)
                {
                    LaunchInstaller(filePath);
                }
            });

            notifs.Show(notification);
        }
        catch
        {
            if (fileSystem.File.Exists(tempPath))
            {
                try
                {
                    fileSystem.File.Delete(tempPath);
                }
                catch
                {
                    // ignore
                }
            }

            throw;
        }
    }

    private static void LaunchInstaller(string filePath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true
        });
    }
    
    private void CleanupTemp()
    {
        try
        {
            string tempFolder = fileSystem.Path.Combine(fileSystem.Path.GetTempPath(), "SkEditor");
            if (!fileSystem.Directory.Exists(tempFolder))
            {
                return;
            }

            IEnumerable<string> filesToDelete = fileSystem.Directory.EnumerateFiles(tempFolder, "Installer-*.*")
                .Where(f => f.EndsWith(".msi", StringComparison.OrdinalIgnoreCase)
                            || f.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));

            foreach (string file in filesToDelete)
            {
                try
                {
                    fileSystem.File.Delete(file);
                }
                catch (Exception ex)
                {
                    logs.Error(ex, $"Failed to delete old installer file: {file}");
                }
            }
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to clean up old installer files");
        }
    }
}