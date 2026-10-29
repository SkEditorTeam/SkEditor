using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using R3;
using SkEditor.Analyzing.Downloading;
using SkEditor.Analyzing.Models;
using SkEditor.Analyzing.Processes;
using SkEditor.Analyzing.Protocol;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Logging;

namespace SkEditor.Analyzing;

public class AnalyzerInitializer(
    ExperimentsConfig experiments,
    IAnalyzer analyzer,
    IAnalyzerDownloadHelper downloader,
    IMessenger messenger,
    ILogService logs,
    IAnalyzerProcessRunner processRunner,
    IAnalyzerProtocolHandler protocolHandler) : IAnalyzerInitializer
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    
    public void ObserveOptionChange()
    {
        experiments
            .ObservePropertyChanged(x => x.AnalyzerEnabled)
            .SubscribeAwait(async (v, _) =>
            {
                if (v)
                {
                    await InitializeAnalyzer();
                }
                else
                {
                    processRunner.Stop();
                    protocolHandler.ResetState();
                    messenger.Send(new AnalyzerUnloadedMessage());
                }
            });
        
        downloader.IsReady
            .ObservePropertyChanged(x => x.Value, pushCurrentValueOnSubscribe: false)
            .SubscribeAwait(async (v, _) =>
            {
                if (v && experiments.AnalyzerEnabled && !analyzer.IsRunning)
                {
                    await InitializeAnalyzer();
                }
            });
    }

    private async Task InitializeAnalyzer()
    {
        if (!await _initLock.WaitAsync(0))
        {
            return;
        }

        try
        {
            if (!downloader.CheckComponents())
            {
                try
                {
                    await downloader.Download();
                }
                catch (Exception ex)
                {
                    logs.Error(ex, "Failed to download Analyzer components.");
                    messenger.Send(new AnalyzerFailedToLoadMessage());
                    return;
                }
            }

            if (!analyzer.IsRunning)
            {
                await analyzer.Initialize();
            }
        }
        finally
        {
            _initLock.Release();
        }
    }
}