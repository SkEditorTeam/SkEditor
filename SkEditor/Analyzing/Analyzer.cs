using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Analyzing.Aot;
using SkEditor.Analyzing.Bootstrapping;
using SkEditor.Analyzing.Processes;
using SkEditor.Analyzing.Protocol;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Analyzer;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Analyzing;

public class Analyzer(
    IConfigService configs,
    ITabService tabs,
    ILogService logs,
    IMessenger messenger,
    IAnalyzerBootstrapper bootstrapper,
    IAnalyzerAotCacheManager aotManager,
    IAnalyzerProcessBuilder processBuilder,
    IAnalyzerProcessRunner processRunner,
    IAnalyzerProtocolHandler protocolHandler) : IAnalyzer, IDisposable
{
    public string AnalyzerRoot => Path.Combine(configs.GetAppDataPath(), "Analyzer");
    public string BinDir => Path.Combine(AnalyzerRoot, "Binaries");
    public string WorkingDir => Path.Combine(AnalyzerRoot, "Server");

    public async Task Initialize()
    {
        string analyzerPath = Path.Combine(BinDir, "SkAnalyzer.jar");
        string aotPath = Path.Combine(BinDir, "skanalyzer.aot");

        Directory.CreateDirectory(WorkingDir);
        Directory.CreateDirectory(BinDir);

        if (!File.Exists(analyzerPath))
        {
            messenger.Send(new AnalyzerFailedToLoadMessage());
            return;
        }

        await bootstrapper.EnsureLibrariesExtractedAsync(analyzerPath, WorkingDir);

        if (!File.Exists(aotPath))
        {
            await aotManager.GenerateAotCacheAsync(WorkingDir, analyzerPath, aotPath);
        }

        processRunner.OutputDataReceived += (_, data) => protocolHandler.HandleOutputData(data);
        processRunner.ErrorDataReceived += (_, data) => protocolHandler.HandleErrorData(data);
        processRunner.ProcessExited += OnProcessExited;
        
        protocolHandler.CorruptedAotDetected += (_, _) =>
        {
            aotManager.CleanupCorruptAot(aotPath);
        };

        protocolHandler.ResetState();

        try
        {
            ProcessStartInfo startInfo = processBuilder.BuildProcessStartInfo(WorkingDir, analyzerPath, aotPath);
            processRunner.Start(startInfo);
        }
        catch (Exception ex)
        {
            logs.Error($"Failed to start process: {ex.Message}");
            messenger.Send(new AnalyzerFailedToLoadMessage());
        }
    }

    public async Task<AnalyzerResult?> Analyze(string text, CancellationToken ct)
    {
        if (!processRunner.IsRunning)
        {
            return null;
        }

        messenger.Send(new AnalyzerStatusChangedMessage(true));

        try
        {
            string tempFilePath = Path.Combine(WorkingDir, "temp_script.sk");
            await File.WriteAllTextAsync(tempFilePath, text, ct);

            AnalyzeResultsDto? resultsDto = await protocolHandler.SendParseCommandAsync(tempFilePath, ct);

            if (resultsDto is not null && tabs.SelectedTab is IFileTab fileTab)
            {
                AnalyzerResult result = AnalyzerResult.FromDto(resultsDto.Results.First().Value);
                messenger.Send(new AnalyzerResultUpdatedMessage(fileTab, result));
                return result;
            }
        }
        finally
        {
            messenger.Send(new AnalyzerStatusChangedMessage(false));
        }

        return null;
    }

    public void Dispose()
    {
        processRunner.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        protocolHandler.ResetState();
        logs.Error("Analyzer process has exited unexpectedly.");
        messenger.Send(new AnalyzerFailedToLoadMessage());
    }
}