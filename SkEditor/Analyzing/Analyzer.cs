using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Analyzing.Aot;
using SkEditor.Analyzing.Bootstrapping;
using SkEditor.Analyzing.Models;
using SkEditor.Analyzing.Processes;
using SkEditor.Analyzing.Protocol;
using SkEditor.Configuration;
using SkEditor.Core.Logging;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Analyzing;

public class Analyzer(
    IConfigService configs,
    ITabService tabs,
    ILogService logs,
    IMessenger messenger,
    IAnalyzerBootstrapper bootstrapper,
    IAnalyzerAotGenerator aotManager,
    IAnalyzerProcessBuilder processBuilder,
    IAnalyzerProcessRunner processRunner,
    IAnalyzerProtocolHandler protocolHandler) : IAnalyzer, IDisposable
{
    private readonly SemaphoreSlim _analyzeLock = new(1, 1);
    private string? _aotPath;
    public string AnalyzerRoot => Path.Combine(configs.GetLocalAppDataPath(), "Analyzer");
    public string BinDir => Path.Combine(AnalyzerRoot, "Binaries");

    public bool IsRunning => processRunner.IsRunning;
    public string WorkingDir => Path.Combine(AnalyzerRoot, "Server");

    public async Task Initialize()
    {
        MigrateDirectory();

        string? javaPath = GetJavaPath();
        string analyzerPath = Path.Combine(BinDir, "SkAnalyzer.jar");
        _aotPath = Path.Combine(BinDir, "skanalyzer.aot");
        string pluginsPath = Path.Combine(WorkingDir, "plugins");

        Directory.CreateDirectory(WorkingDir);
        Directory.CreateDirectory(BinDir);
        Directory.CreateDirectory(pluginsPath);

        if (!File.Exists(javaPath) || !File.Exists(analyzerPath))
        {
            messenger.Send(new AnalyzerFailedToLoadMessage());
            return;
        }

        messenger.Send(new AnalyzerUnloadedMessage());

        string[]? libraryPaths = await bootstrapper.EnsureLibrariesExtractedAsync(analyzerPath, WorkingDir);
        
        if (libraryPaths is null || libraryPaths.Length == 0)
        {
            logs.Error("Failed to extract libraries for SkAnalyzer.", LogSource.Analyzer);
            messenger.Send(new AnalyzerFailedToLoadMessage());
            return;
        }

        if (!File.Exists(_aotPath))
        {
            await aotManager.GenerateAotCacheAsync(WorkingDir, javaPath, analyzerPath, _aotPath, libraryPaths);
        }

        UnsubscribeEvents();
        SubscribeEvents();

        protocolHandler.ResetState();

        try
        {
            ProcessStartInfo startInfo = processBuilder.BuildProcessStartInfo(WorkingDir, javaPath, analyzerPath, _aotPath, libraryPaths);
            processRunner.Start(startInfo);
        }
        catch (Exception ex)
        {
            logs.Error($"Failed to start process: {ex.Message}", LogSource.Analyzer);
            messenger.Send(new AnalyzerFailedToLoadMessage());
        }
    }

    private string? GetJavaPath()
    {
        string jreFolderPath = Path.Combine(BinDir, "JRE");

        if (!Directory.Exists(jreFolderPath))
        {
            return null;
        }

        string? jdkFolder = Directory
            .EnumerateDirectories(jreFolderPath)
            .FirstOrDefault(d => Path.GetFileName(d).StartsWith("jdk"));

        if (jdkFolder is null)
        {
            return null;
        }
        
        string executablePath;
        if (OperatingSystem.IsMacOS())
        {
            executablePath = Path.Combine(jdkFolder, "Contents", "Home", "bin", "java");
        }
        else
        {
            string executableName = OperatingSystem.IsWindows() ? "java.exe" : "java";
            executablePath = Path.Combine(jdkFolder, "bin", executableName);
        }

        return !File.Exists(executablePath) ? null : executablePath;
    }

    // TEMP: Remove migration from Roaming when leaving Alpha
    private void MigrateDirectory()
    {
        try
        {
            string legacyRoot = Path.Combine(configs.GetAppDataPath(), "Analyzer");

            if (Directory.Exists(AnalyzerRoot) || !Directory.Exists(legacyRoot))
            {
                return;
            }

            Directory.Move(legacyRoot, AnalyzerRoot);
            logs.Info("Migrated Analyzer directory from Roaming to LocalApplicationData.", LogSource.Analyzer);
        }
        catch
        {
            logs.Error("Failed to migrate Analyzer directory from Roaming to LocalApplicationData.", LogSource.Analyzer);
        }
    }

    public async Task<AnalyzerResult?> Analyze(string text, CancellationToken ct)
    {
        if (!processRunner.IsRunning)
        {
            return null;
        }

        await _analyzeLock.WaitAsync(ct);
        messenger.Send(new AnalyzerStatusChangedMessage(true));

        string tempFilePath = Path.Combine(WorkingDir, $"temp_{Guid.NewGuid():N}.sk");
        try
        {
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
            File.Delete(tempFilePath);
            _analyzeLock.Release();
            messenger.Send(new AnalyzerStatusChangedMessage(false));
        }

        return null;
    }

    private void SubscribeEvents()
    {
        processRunner.OutputDataReceived += OnOutputDataReceived;
        processRunner.ErrorDataReceived += OnErrorDataReceived;
        processRunner.ProcessExited += OnProcessExited;
        protocolHandler.CorruptedAotDetected += OnCorruptedAotDetected;
    }

    private void UnsubscribeEvents()
    {
        processRunner.OutputDataReceived -= OnOutputDataReceived;
        processRunner.ErrorDataReceived -= OnErrorDataReceived;
        processRunner.ProcessExited -= OnProcessExited;
        protocolHandler.CorruptedAotDetected -= OnCorruptedAotDetected;
    }

    private void OnOutputDataReceived(object? sender, string data)
    {
        protocolHandler.HandleOutputData(data);
    }

    private void OnErrorDataReceived(object? sender, string data)
    {
        protocolHandler.HandleErrorData(data);
    }

    private void OnCorruptedAotDetected(object? sender, EventArgs e)
    {
        if (_aotPath != null)
        {
            aotManager.CleanupCorruptAot(_aotPath);
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        protocolHandler.ResetState();
        logs.Error("Analyzer process has exited unexpectedly.", LogSource.Analyzer);
        messenger.Send(new AnalyzerFailedToLoadMessage());
    }

    public void Dispose()
    {
        UnsubscribeEvents();
        processRunner.Dispose();
        GC.SuppressFinalize(this);
    }
}