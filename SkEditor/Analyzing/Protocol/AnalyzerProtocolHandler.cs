using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Analyzing.Processes;
using SkEditor.Contracts.Services;
using SkEditor.Models.Analyzer;
using SkEditor.Models.Logging;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;

namespace SkEditor.Analyzing.Protocol;

public class AnalyzerProtocolHandler(
    ILogService logs,
    INotificationService notifs,
    IMessenger messenger,
    IAnalyzerProcessRunner processRunner) : IAnalyzerProtocolHandler
{
    private TaskCompletionSource<AnalyzeResultsDto?>? _analysisTcs;
    private bool _didRemoveCorruptedAot;
    private bool _isInitialized;

    public async Task<AnalyzeResultsDto?> SendParseCommandAsync(string scriptFilePath, CancellationToken ct)
    {
        _analysisTcs = new TaskCompletionSource<AnalyzeResultsDto?>();

        await using CancellationTokenRegistration registration = ct.Register(() => _analysisTcs.TrySetCanceled());

        try
        {
            string normalizedPath = scriptFilePath.NormalizePathSeparators();
            await processRunner.WriteInputLineAsync($"parse {normalizedPath}");

            return await _analysisTcs.Task;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to send command to analyzer process.", LogSource.Analyzer);
            return null;
        }
    }

    public void HandleOutputData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        if (!_isInitialized && !_didRemoveCorruptedAot && data.Contains("[aot]") &&
            (data.Contains("error") || data.Contains("bad magic number")))
        {
            logs.Warning("Detected corrupted AOT cache during JVM startup. Deleting bad .aot file...",
                LogSource.Analyzer);
            CorruptedAotDetected?.Invoke(this, EventArgs.Empty);
            _didRemoveCorruptedAot = true;
        }

        if (data.Contains("Successfully enabled") || data.Contains("Successfully started"))
        {
            _isInitialized = true;
            messenger.Send(new AnalyzerLoadedMessage());
        }

        if (!data.Contains("{\"results\":"))
        {
            logs.LogAnalyzerLine(data);
            return;
        }

        try
        {
            int jsonStartIndex = data.IndexOf("{\"results\":", StringComparison.Ordinal);
            string json = data[jsonStartIndex..];

            AnalyzeResultsDto? result =
                JsonSerializer.Deserialize(json, AnalyzeResultContext.Default.AnalyzeResultsDto);
            _analysisTcs?.TrySetResult(result);
        }
        catch (Exception ex)
        {
            _analysisTcs?.TrySetException(ex);
        }
    }

    public void ResetState()
    {
        _isInitialized = false;
        _didRemoveCorruptedAot = false;
        _analysisTcs = null;
    }

    public void HandleErrorData(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return;
        }

        if (data.StartsWith("WARNING", StringComparison.OrdinalIgnoreCase))
        {
            logs.Warning(data, LogSource.Analyzer);
            return;
        }

        Notification notification = Notification.Create(TranslationKey.AnalyzerErrorTitle)
            .WithMessage(data.ToTranslatable())
            .WithType(NotificationType.Error);

        notifs.Show(notification);
        logs.Error(data, LogSource.Analyzer);
    }

    public event EventHandler? CorruptedAotDetected;
}