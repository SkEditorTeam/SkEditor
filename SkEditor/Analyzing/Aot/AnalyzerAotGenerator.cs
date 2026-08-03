using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Analyzing.Processes;
using SkEditor.Contracts.Services;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Extensions;

namespace SkEditor.Analyzing.Aot;

public class AnalyzerAotGenerator(
    ILogService logs,
    INotificationService notifs,
    IAnalyzerProcessBuilder processBuilder) : IAnalyzerAotGenerator
{
    public async Task GenerateAotCacheAsync(string workingDir, string analyzerPath, string aotPath,
        CancellationToken ct = default)
    {
        Notification notification = Notification
            .Create("Analyzer AOT Cache".ToTranslatable())
            .WithMessage("Generating AOT cache for the Analyzer. This is a one-time operation and may take a bit longer."
                .ToTranslatable())
            .WithDuration(TimeSpan.FromSeconds(10))
            .WithType(NotificationType.Information);

        notifs.Show(notification);

        TaskCompletionSource<bool> enabledTcs = new();
        ProcessStartInfo startInfo =
            processBuilder.BuildProcessStartInfo(workingDir, analyzerPath, aotPath, isGeneratingAot: true);

        Process process = new() { StartInfo = startInfo };

        try
        {
            process.OutputDataReceived += (_, args) =>
            {
                if (string.IsNullOrEmpty(args.Data)) return;

                if (args.Data.Contains("Successfully enabled") || args.Data.Contains("Successfully started"))
                {
                    enabledTcs.TrySetResult(true);
                }
            };

            process.ErrorDataReceived += (_, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    logs.Warning($"An error occurred during AOT generation: {args.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

            await using CancellationTokenRegistration
                reg = timeoutCts.Token.Register(() => enabledTcs.TrySetCanceled());

            try
            {
                await enabledTcs.Task;

                await process.StandardInput.WriteLineAsync("exit");
                await process.WaitForExitAsync(timeoutCts.Token);

                if (File.Exists(aotPath) && new FileInfo(aotPath).Length > 1000)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                logs.Error(ex, "Failed to generate AOT cache.");
            }

            CleanupCorruptAot(aotPath);
        }
        catch (Exception ex)
        {
            logs.Error(ex, "An error occurred during AOT generation.");
            CleanupCorruptAot(aotPath);
        }
        finally
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                process.Dispose();
            }
        }
    }

    public void CleanupCorruptAot(string aotPath)
    {
        if (!File.Exists(aotPath)) return;

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