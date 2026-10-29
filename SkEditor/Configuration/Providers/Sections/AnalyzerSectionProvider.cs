using System;
using System.IO;
using System.Threading.Tasks;
using FluentIcons.Common;
using R3;
using SkEditor.Analyzing;
using SkEditor.Analyzing.Downloading;
using SkEditor.Configuration.Configs;
using SkEditor.Configuration.Models;
using SkEditor.Configuration.Models.Types;
using SkEditor.Core.Common;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Core.System;
using ExperimentsProperty = SkEditor.Configuration.Configs.ExperimentsConfig.ExperimentsConfigProperty;

namespace SkEditor.Configuration.Providers.Sections;

public class AnalyzerSectionProvider(
    ExperimentsConfig experiments,
    ILinkService links,
    IAnalyzer analyzer,
    IAnalyzerDownloadHelper downloader,
    INotificationService notifs,
    ILogService logs) : ISettingsSectionProvider
{
    public string ParentId => "Editor";

    public SettingSection GetSection()
    {
        BindableReactiveProperty<bool> isReady = new(downloader.CheckComponents());

        isReady.SubscribeAwait(async (v, _) =>
        {
            if (!v || analyzer.IsRunning || !experiments.AnalyzerEnabled)
            {
                return;
            }

            try
            {
                await analyzer.Initialize();
            }
            catch (Exception ex)
            {
                Notification notification = Notification
                    .Create(TranslationKey.AnalyzerFailedToLoad)
                    .WithMessage(TranslationKey.AnalyzerGeneralFailureMessage)
                    .WithType(NotificationType.Error);
                notifs.Show(notification);

                logs.Error(ex, "Failed to initialize Analyzer process.");
            }
        }, AwaitOperation.Drop);

        BindableReactiveProperty<TranslatableString> everythingTitle = isReady
            .Select(GetComponentStatusTitle)
            .ToBindableReactiveProperty(GetComponentStatusTitle(isReady.Value));

        SettingSection section = SettingSection.Create("Analyzer", TranslationKey.SettingsSectionAnalyzer)
            .WithIcon(Symbol.DocumentSearch)
            .WithSettings(
            [
                InfoSetting
                    .Create(TranslationKey.Attention)
                    .WithDescription(TranslationKey.SettingsAnalyzerWarning)
                    .WithIcon(Symbol.Warning)
                    .WithType(InfoSetting.InfoType.Warning),

                ActionSetting
                    .Create(everythingTitle)
                    .WithStatus(isReady)
                    .WithButtonVisible(isReady.Inverse())
                    .WithAction("Setup".ToTranslatable(), async () =>
                    {
                        await SetupComponents(isReady);
                    }),

                GetToolsSetting(isReady),

                ToggleSetting
                    .Create(TranslationKey.SettingsAnalyzerScrollbarErrorMarkers)
                    .WithDescription(TranslationKey.SettingsAnalyzerScrollbarErrorMarkersDescription)
                    .WithIcon(Symbol.SidebarSearch)
                    .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.AnalyzerErrorScrollMarkersEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsAnalyzerDiagnosticTooltips)
                    .WithDescription(TranslationKey.SettingsAnalyzerDiagnosticTooltipsDescription)
                    .WithIcon(Symbol.CommentNote)
                    .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.AnalyzerErrorTooltipsEnabled))
            ]);

        section.IsVisible = experiments
            .ObservePropertyChanged(x => x.AnalyzerEnabled)
            .ToBindableReactiveProperty();

        return section;
    }

    private async Task<bool> SetupComponents(BindableReactiveProperty<bool> isReady)
    {
        try
        {
            bool preCheck = downloader.CheckComponents();
            if (preCheck)
            {
                isReady.Value = preCheck;
                return true;
            }
            
            await downloader.Download();
            bool check = downloader.CheckComponents();
            isReady.Value = check;
            return check;
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to download Analyzer components.");
            return false;
        }
    }

    private static TranslatableString GetComponentStatusTitle(bool ready)
    {
        return ready
            ? TranslationKey.SettingsAnalyzerComponentsInstalled
            : TranslationKey.SettingsAnalyzerComponentsMissing;
    }

    private ExpandableSetting GetToolsSetting(BindableReactiveProperty<bool> isReady)
    {
        string pluginsDir = Path.Combine(analyzer.WorkingDir, "plugins");
        
        return ExpandableSetting
            .Create(TranslationKey.SettingsAnalyzerToolsTitle)
            .WithDescription(TranslationKey.SettingsAnalyzerToolsDescription)
            .WithIcon(Symbol.Wrench)
            .WithChildren(
                ActionSetting
                    .Create(TranslationKey.SettingsAnalyzerPluginsFolder)
                    .WithAction(TranslationKey.ButtonOpenFolder, () =>
                    {
                        Directory.CreateDirectory(pluginsDir);
                        links.OpenFolder(pluginsDir);
                    }),
                ActionSetting
                    .Create(TranslationKey.SettingsAnalyzerRestartTitle)
                    .WithDescription(TranslationKey.SettingsAnalyzerRestartDescription)
                    .WithAction(TranslationKey.ButtonRestart, async () =>
                    {
                        try
                        {
                            bool success = await SetupComponents(isReady);
                            if (success)
                            {
                                await analyzer.Initialize();
                            }
                        }
                        catch (Exception ex)
                        {
                            Notification notification = Notification
                                .Create(TranslationKey.SettingsAnalyzerRestartFailedTitle)
                                .WithMessage(TranslationKey.SettingsAnalyzerRestartFailedMessage)
                                .WithType(NotificationType.Error);
                            notifs.Show(notification);

                            logs.Error(ex, "Failed to restart the Analyzer process.");
                        }
                    }),
                ActionSetting
                    .Create(TranslationKey.SettingsAnalyzerClearAotCacheTitle)
                    .WithDescription(TranslationKey.SettingsAnalyzerClearAotCacheDescription)
                    .WithAction(TranslationKey.ButtonClear, () =>
                    {
                        string aotPath = Path.Combine(analyzer.BinDir, "skanalyzer.aot");
                        if (!File.Exists(aotPath))
                        {
                            return;
                        }

                        try
                        {
                            File.Delete(aotPath);

                            Notification notification = Notification
                                .Create(TranslationKey.SettingsAnalyzerClearAotCacheSuccessTitle)
                                .WithMessage(TranslationKey.SettingsAnalyzerClearAotCacheSuccessMessage)
                                .WithType(NotificationType.Success);
                            notifs.Show(notification);
                        }
                        catch (Exception ex)
                        {
                            Notification notification = Notification
                                .Create(TranslationKey.SettingsAnalyzerClearAotCacheFailedTitle)
                                .WithMessage(TranslationKey.SettingsAnalyzerClearAotCacheFailedMessage)
                                .WithType(NotificationType.Error);
                            notifs.Show(notification);

                            logs.Error(ex, "Failed to delete AOT cache.");
                        }
                    })
            );
    }
}