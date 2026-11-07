using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Analyzing.Processes;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Core.Localization;

namespace SkEditor.Analyzing.Aot;

public class AnalyzerAotGenerator(
    ILogService logs,
    INotificationService notifs,
    IAnalyzerProcessBuilder processBuilder) : IAnalyzerAotGenerator
{
    public async Task GenerateAotCacheAsync(string workingDir, string javaPath, string analyzerPath, string aotPath, string[] libraryPaths,
        CancellationToken ct = default)
    {
        Notification notification = Notification
            .Create(TranslationKey.AnalyzerAotCacheGeneratingTitle)
            .WithMessage(TranslationKey.AnalyzerAotCacheGeneratingMessage)
            .WithDuration(TimeSpan.FromSeconds(10))
            .WithType(NotificationType.Information);

        notifs.Show(notification);

        TaskCompletionSource<bool> enabledTcs = new();
        ProcessStartInfo startInfo =
            processBuilder.BuildProcessStartInfo(workingDir, javaPath, analyzerPath, aotPath, libraryPaths, isGeneratingAot: true);

        Process process = new() { StartInfo = startInfo };

        try
        {
            process.OutputDataReceived += (_, args) =>
            {
                if (string.IsNullOrEmpty(args.Data))
                {
                    return;
                }

                if (args.Data.Contains("Successfully enabled") || args.Data.Contains("Successfully started"))
                {
                    enabledTcs.TrySetResult(true);
                }
            };

            process.ErrorDataReceived += (_, args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data))
                {
                    return;
                }

                if (args.Data.StartsWith("Picked up JAVA_TOOL_OPTIONS", StringComparison.OrdinalIgnoreCase) ||
                    args.Data.StartsWith("Picked up _JAVA_OPTIONS", StringComparison.OrdinalIgnoreCase))
                {
                    logs.Debug(args.Data, LogSource.Analyzer);
                    return;
                }

                logs.Warning($"An error occurred during AOT generation: {args.Data}", LogSource.Analyzer);
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
                logs.Error(ex, "Failed to generate AOT cache.", LogSource.Analyzer);
            }

            CleanupCorruptAot(aotPath);
        }
        catch (Exception ex)
        {
            logs.Error(ex, "An error occurred during AOT generation.", LogSource.Analyzer);
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
            catch (Exception ex)
            {
                logs.Error(ex, "Failed to kill the AOT generation process.", LogSource.Analyzer);
            }
            finally
            {
                process.Dispose();
            }
        }
    }

    public void CleanupCorruptAot(string aotPath)
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
            logs.Error(ex, "Failed to delete corrupt AOT file.", LogSource.Analyzer);
        }
    }
}