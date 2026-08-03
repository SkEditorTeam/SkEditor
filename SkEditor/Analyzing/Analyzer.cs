using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Analyzer;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Analyzer;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Extensions;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Analyzing;

public class Analyzer(
    IConfigService configs,
    ITabService tabs,
    INotificationService notifs,
    IMessenger messenger,
    ILogService logs) : IAnalyzer, IDisposable
{
    public readonly string AnalyzerRoot = Path.Combine(configs.GetAppDataPath(), "Analyzer");
    private TaskCompletionSource<AnalyzeResultsDto?>? _analysisTcs;

    private Process? _process;

    public string BinDir => Path.Combine(AnalyzerRoot, "Binaries");
    public string WorkingDir => Path.Combine(AnalyzerRoot, "Server");

    public async Task Initialize()
    {
        string analyzerPath = Path.Combine(BinDir, "SkAnalyzer.jar");
        string aotPath = Path.Combine(BinDir, "skanalyzer.aot");

        if (!await CheckAnalyzerJar(analyzerPath))
        {
            return;
        }

        await AnalyzerBootstrap.EnsureLibrariesExtractedAsync(analyzerPath, WorkingDir, logs);

        if (!File.Exists(aotPath))
        {
            await AnalyzerAotGenerator.GenerateAotCacheAsync(WorkingDir, analyzerPath, aotPath, logs, notifs);
        }

        _process = new Process
        {
            StartInfo = AnalyzerProcessBuilder.BuildProcessStartInfo(WorkingDir, analyzerPath, aotPath)
        };

        _process.ErrorDataReceived += OnErrorReceived;
        _process.OutputDataReceived += OnOutputReceived;

        try
        {
            _process.Start();
        }
        catch (Exception ex)
        {
            logs.Error($"Failed to start process: {ex.Message}");
            _process = null;

            await Task.Delay(2500);

            messenger.Send(new AnalyzerFailedToLoadMessage());

            Notification notification = Notification
                .Create("Analyzer failed to start".ToTranslatable())
                .WithMessage("Failed to start the Analyzer process. Look into the logs for more information."
                    .ToTranslatable())
                .WithDuration(TimeSpan.MaxValue)
                .WithType(NotificationType.Error);

            notifs.Show(notification);
            return;
        }

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
    }

    public async Task<AnalyzerResult?> Analyze(string text, CancellationToken ct)
    {
        if (_process is null)
        {
            return null;
        }

        messenger.Send(new AnalyzerStatusChangedMessage(true));

        string tempFilePath = Path.Combine(WorkingDir, "temp_script.sk");
        await File.WriteAllTextAsync(tempFilePath, text, ct);

        _analysisTcs = new TaskCompletionSource<AnalyzeResultsDto?>();

        await using CancellationTokenRegistration registration = ct.Register(() => _analysisTcs.TrySetCanceled());

        try
        {
            await _process.StandardInput.WriteLineAsync($"parse {tempFilePath.NormalizePathSeparators()}");

            AnalyzeResultsDto? resultsDto = await _analysisTcs.Task;

            if (resultsDto is not null && tabs.SelectedTab is IFileTab fileTab)
            {
                AnalyzerResult result = AnalyzerResult.FromDto(resultsDto.Results.First().Value);
                messenger.Send(new AnalyzerResultUpdatedMessage(fileTab, result));
                return result;
            }
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (IOException ex)
        {
            if (_process.HasExited)
            {
                logs.Error(ex, "Analyzer process has exited unexpectedly");
                _process = null;
                messenger.Send(new AnalyzerFailedToLoadMessage());

                notifs.Show(Notification
                    .Create("Analyzer process has exited".ToTranslatable())
                    .WithMessage("The Analyzer process has exited unexpectedly. Please restart SkEditor."
                        .ToTranslatable())
                    .MakePersistent()
                    .WithType(NotificationType.Error));

                return null;
            }
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to send command to analyzer process");
            return null;
        }
        finally
        {
            messenger.Send(new AnalyzerStatusChangedMessage(false));
        }

        return null;
    }

    public void Dispose()
    {
        if (_process is null || _process.HasExited)
        {
            return;
        }

        GC.SuppressFinalize(this);

        try
        {
            _process.Kill(true);
            _process.Dispose();
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to kill Analyzer process on exit");
        }
    }

    private async Task<bool> CheckAnalyzerJar(string analyzerPath)
    {
        Directory.CreateDirectory(WorkingDir);
        Directory.CreateDirectory(BinDir);

        if (File.Exists(analyzerPath))
        {
            return true;
        }

        messenger.Send(new AnalyzerFailedToLoadMessage());
        await Task.Delay(2500);

        Notification notification = Notification
            .Create("Missing SkAnalyzer.jar".ToTranslatable())
            .WithMessage(
                "The SkAnalyzer.jar file is missing. Please refer to the installation instructions on the SkEditor Discord server."
                    .ToTranslatable())
            .WithDuration(TimeSpan.MaxValue)
            .WithType(NotificationType.Error);

        notifs.Show(notification);
        return false;
    }

    private void OnErrorReceived(object _, DataReceivedEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.Data))
        {
            return;
        }

        if (args.Data.StartsWith("WARNING", StringComparison.OrdinalIgnoreCase))
        {
            logs.Warning($"Analyzer Warning: {args.Data}");
            return;
        }

        Notification notification = Notification.Create("Analyzer Error".ToTranslatable())
            .WithMessage((args.Data ?? string.Empty).ToTranslatable())
            .WithType(NotificationType.Error);

        notifs.Show(notification);
        logs.Error($"Analyzer Error: {args.Data}");
    }

    private void OnOutputReceived(object _, DataReceivedEventArgs args)
    {
        if (string.IsNullOrEmpty(args.Data))
        {
            return;
        }

        if (args.Data.Contains("Successfully enabled") || args.Data.Contains("Successfully started"))
        {
            messenger.Send(new AnalyzerLoadedMessage());
        }

        if (!args.Data.Contains("{\"results\":"))
        {
            return;
        }

        try
        {
            int jsonStartIndex = args.Data.IndexOf("{\"results\":", StringComparison.Ordinal);
            string json = args.Data[jsonStartIndex..];

            AnalyzeResultsDto? result =
                JsonSerializer.Deserialize<AnalyzeResultsDto>(json, AnalyzeResultContext.Default.AnalyzeResultsDto);

            _analysisTcs?.TrySetResult(result);
        }
        catch (Exception ex)
        {
            _analysisTcs?.TrySetException(ex);
        }
    }
}