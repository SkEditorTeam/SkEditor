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
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Core.System;
using SkEditor.Workspace.Files;
using ExperimentsProperty = SkEditor.Configuration.Configs.ExperimentsConfig.ExperimentsConfigProperty;

namespace SkEditor.Configuration.Providers.Sections;

public class AnalyzerSectionProvider(
    ExperimentsConfig experiments,
    ILinkService links,
    IAnalyzer analyzer,
    IAnalyzerDownloadHelper downloader,
    INotificationService notifs,
    IFilePickerService filePicker,
    ILogService logs) : ISettingsSectionProvider
{
    public string ParentId => "Editor";

    public SettingSection GetSection()
    {
        SettingSection section = SettingSection.Create("Analyzer", TranslationKey.SettingsSectionAnalyzer)
            .WithIcon(Symbol.DocumentSearch)
            .WithSettings(
            [
                InfoSetting
                    .Create(TranslationKey.Attention)
                    .WithDescription(TranslationKey.SettingsAnalyzerWarning)
                    .WithIcon(Symbol.Warning)
                    .WithType(InfoSetting.InfoType.Warning),

                GetReadySetting(),
                GetToolsSetting(),

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

    private SettingElement GetReadySetting()
    {
        BindableReactiveProperty<TranslatableString> everythingTitle = downloader.IsReady
            .Select(GetComponentStatusTitle)
            .ToBindableReactiveProperty(GetComponentStatusTitle(downloader.IsReady.Value));
        
        BindableReactiveProperty<bool> canSetup = downloader.IsReady
            .CombineLatest(downloader.IsDownloading, (ready, downloading) => !ready && !downloading)
            .ToBindableReactiveProperty();
        
        return CompoundSetting
            .Create(everythingTitle)
            .WithStatus(downloader.IsReady)
            .WithSpacing(16)
            .WithChildren(
                ProgressSetting
                    .Create("Progress".ToTranslatable())
                    .WithVisible(downloader.IsDownloading)
                    .WithValue(downloader.Progress)
                    .WithText(downloader.ProgressDescription)
                    .WithWidth(150),

                ActionSetting
                    .Create(TranslationKey.SettingsAnalyzerSetup)
                    .WithButtonVisible(canSetup)
                    .WithAction(TranslationKey.SettingsAnalyzerSetup, async () =>
                    {
                        await SetupComponents();
                    })
            );
    }
    
    private async Task SetupComponents()
    {
        try
        {
            bool preCheck = downloader.CheckComponents();
            if (preCheck)
            {
                return;
            }

            await downloader.Download();
        }
        catch (Exception ex)
        {
            Notification notification = Notification
                .Create(TranslationKey.SettingsAnalyzerDownloadFailed)
                .WithMessage(TranslationKey.SettingsAnalyzerDownloadFailedMessage)
                .WithType(NotificationType.Error);
            notifs.Show(notification);

            logs.Error(ex, "Failed to download Analyzer components.");
        }
    }

    private static TranslatableString GetComponentStatusTitle(bool ready)
    {
        return ready
            ? TranslationKey.SettingsAnalyzerComponentsInstalled
            : TranslationKey.SettingsAnalyzerComponentsMissing;
    }

    private ExpandableSetting GetToolsSetting()
    {
        string pluginsDir = Path.Combine(analyzer.WorkingDir, "plugins");

        return ExpandableSetting
            .Create(TranslationKey.SettingsAnalyzerToolsTitle)
            .WithDescription(TranslationKey.SettingsAnalyzerToolsDescription)
            .WithIcon(Symbol.Wrench)
            .WithChildren(
                ActionSetting
                    .Create(TranslationKey.SettingsAnalyzerAddPluginTitle)
                    .WithDescription(TranslationKey.SettingsAnalyzerAddPluginDescription)
                    .WithAction(TranslationKey.ButtonAdd, async () =>
                    {
                        await AddPlugin(pluginsDir);
                    }),
                
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
                            if (downloader.CheckComponents())
                            {
                                await analyzer.Initialize();
                            }
                            else
                            {
                                await SetupComponents();
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
    
    private async Task AddPlugin(string pluginsDir)
    {
        try
        {
            string[]? files = await filePicker.PickFilesAsync(
                title: TranslationKey.SettingsAnalyzerAddPluginDialogTitle,
                allowMultiple: true);

            if (files is null || files.Length == 0)
            {
                return;
            }

            Directory.CreateDirectory(pluginsDir);

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string dest = Path.Combine(pluginsDir, fileName);
                File.Copy(file, dest, overwrite: true);
            }

            if (downloader.CheckComponents())
            {
                await analyzer.Initialize();
            }
            else
            {
                await SetupComponents();
            }

            Notification notification = Notification
                .Create(TranslationKey.SettingsAnalyzerAddPluginSuccessTitle)
                .WithMessage(TranslationKey.SettingsAnalyzerAddPluginSuccessMessage)
                .WithType(NotificationType.Success);
            notifs.Show(notification);
        }
        catch (Exception ex)
        {
            Notification notification = Notification
                .Create(TranslationKey.SettingsAnalyzerAddPluginFailedTitle)
                .WithMessage(TranslationKey.SettingsAnalyzerAddPluginFailedMessage)
                .WithType(NotificationType.Error);
            notifs.Show(notification);

            logs.Error(ex, "Failed to install plugin(s) and restart Analyzer.");
        }
    }
}