using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Extensions;

namespace SkEditor.Analyzing;

internal static class AnalyzerAotGenerator
{
    internal static async Task GenerateAotCacheAsync(string workingDir, string analyzerPath, string aotPath, ILogService logs, INotificationService notifs)
    {
        Notification notification = Notification
            .Create("Generating AOT Cache".ToTranslatable())
            .WithMessage("Generating AOT cache for the Analyzer. This is a one-time operation and may take some time."
                .ToTranslatable())
            .WithDuration(TimeSpan.MaxValue)
            .WithType(NotificationType.Information);

        notifs.Show(notification);

        TaskCompletionSource<bool> enabledTcs = new();
        ProcessStartInfo startInfo = AnalyzerProcessBuilder.BuildProcessStartInfo(workingDir, analyzerPath, aotPath, true);

        Process p = new() { StartInfo = startInfo };
        try
        {
            p.OutputDataReceived += (_, args) =>
            {
                if (string.IsNullOrEmpty(args.Data)) return;

                if (args.Data.Contains("Successfully enabled") || args.Data.Contains("Successfully started"))
                {
                    enabledTcs.TrySetResult(true);
                }
            };

            p.ErrorDataReceived += (_, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    logs.Warning($"An error occurred during AOT generation: {args.Data}");
                }
            };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            using CancellationTokenSource timeoutCts = new(TimeSpan.FromSeconds(60));
            await using CancellationTokenRegistration
                reg = timeoutCts.Token.Register(() => enabledTcs.TrySetCanceled());

            try
            {
                await enabledTcs.Task;

                await p.StandardInput.WriteLineAsync("exit");
                await p.WaitForExitAsync(timeoutCts.Token);

                if (File.Exists(aotPath) && new FileInfo(aotPath).Length > 1000)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                logs.Error(ex, "Failed to generate AOT cache.");
            }

            CleanupCorruptAot(aotPath, logs);
        }
        catch (Exception ex)
        {
            logs.Error(ex, "An error occurred during AOT generation.");
            CleanupCorruptAot(aotPath, logs);
        }
        finally
        {
            try
            {
                if (!p.HasExited)
                {
                    p.Kill(true);
                }
            }
            catch
            {
                // ignored
            }

            p.Dispose();
        }
    }

    private static void CleanupCorruptAot(string aotPath, ILogService logs)
    {
        if (!File.Exists(aotPath))
        {
            return;
        }

        try
        {
            File.Delete(aotPath);
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to delete corrupt AOT file.");
        }
    }
}