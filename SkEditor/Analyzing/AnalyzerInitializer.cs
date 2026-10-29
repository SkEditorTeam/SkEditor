using System;
using CommunityToolkit.Mvvm.Messaging;
using R3;
using SkEditor.Analyzing.Downloading;
using SkEditor.Analyzing.Models;
using SkEditor.Analyzing.Processes;
using SkEditor.Analyzing.Protocol;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Core.Notifications.Messages;

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
    public void ObserveOptionChange()
    {
        experiments
            .ObservePropertyChanged(x => x.AnalyzerEnabled)
            .SubscribeAwait(async (v, _) =>
            {
                if (v)
                {
                    if (!downloader.CheckComponents())
                    {
                        try
                        {
                            await downloader.Download();
                        }
                        catch (Exception ex)
                        {
                            Notification notification = Notification
                                .Create(TranslationKey.SettingsAnalyzerDownloadFailed)
                                .WithMessage(TranslationKey.SettingsAnalyzerDownloadFailedMessage)
                                .WithType(NotificationType.Error);

                            messenger.Send(new ShowNotificationMessage(notification));

                            logs.Error(ex, "Failed to download Analyzer components.");
                            return;
                        }
                    }

                    await analyzer.Initialize();
                }
                else
                {
                    processRunner.Stop();
                    protocolHandler.ResetState();
                    messenger.Send(new AnalyzerUnloadedMessage());
                }
            });
    }
}