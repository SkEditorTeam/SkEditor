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
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Analyzing;

public class Analyzer(
    IConfigService configs,
    ITabService tabs,
    INotificationService notifs,
    IMessenger messenger,
    ILogService logs) : IAnalyzer
{
    public readonly string WorkingDir = Path.Combine(configs.GetAppDataPath(), "Analyzer");

    private Process? _process;
    private TaskCompletionSource<AnalyzeResultsDto?>? _analysisTcs;

    public async Task Initialize()
    {
        const string analyzerPath = @"C:\Users\Notro\Desktop\Analyzer\SkAnalyzer3.jar";
        
        Directory.CreateDirectory(WorkingDir);

        ProcessStartInfo startInfo = new()
        {
            FileName = "java",
            Arguments = $"-Dskanalyzer.parentProcess={Environment.ProcessId} -jar \"{analyzerPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = WorkingDir
        };

        _process = new Process
        {
            StartInfo = startInfo
        };

        _process.ErrorDataReceived += (_, args) =>
        {
            Notification notification = Notification
                .Create(TranslatableString.FromRawString("Analyzer Error"))
                .WithMessage(TranslatableString.FromRawString(args.Data ?? string.Empty))
                .WithType(NotificationType.Error);

            notifs.Show(notification);
        };

        _process.OutputDataReceived += (_, args) =>
        {
            if (string.IsNullOrEmpty(args.Data))
            {
                return;
            }

            if (args.Data.Contains("Successfully enabled"))
            {
                messenger.Send(new AnalyzerLoadedMessage());
            }

            if (args.Data.Contains("{\"results\":"))
            {
                try
                {
                    int jsonStartIndex = args.Data.IndexOf("{\"results\":", StringComparison.Ordinal);
                    string json = args.Data[jsonStartIndex..];

                    AnalyzeResultsDto? result =
                        JsonSerializer.Deserialize<AnalyzeResultsDto>(json,
                            AnalyzeResultContext.Default.AnalyzeResultsDto);

                    _analysisTcs?.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    _analysisTcs?.TrySetException(ex);
                }
            }
        };

        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        await _process.WaitForExitAsync();
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
        catch (Exception ex)
        {
            logs.Error($"Failed to send command to analyzer process: {ex.Message}");
            return null;
        }
        finally
        {
            messenger.Send(new AnalyzerStatusChangedMessage(false));
        }

        return null;
    }
}