using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentIcons.Common;
using R3;
using SkEditor.Analyzing;
using SkEditor.Configuration.Configs;
using SkEditor.Configuration.Models;
using SkEditor.Configuration.Models.Types;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Core.System;
using SkEditor.Core.Localization;
using SkEditor.Workspace.Files;
using ExperimentsProperty = SkEditor.Configuration.Configs.ExperimentsConfig.ExperimentsConfigProperty;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Configuration.Providers.Sections;

public class AnalyzerSectionProvider(
    ExperimentsConfig experiments,
    ILinkService links,
    IAnalyzer analyzer,
    IFilePickerService filePicker,
    INotificationService notifs,
    ILogService logs) : ISettingsSectionProvider
{
    public string ParentId => "Editor";

    public SettingSection GetSection()
    {
        string pluginsDir = Path.Combine(analyzer.WorkingDir, "plugins");

        BindableReactiveProperty<bool> isJavaValid = new(false);
        _ = Task.Run(async () => { isJavaValid.Value = await CheckJava25(); });

        BindableReactiveProperty<bool> isAnalyzerPresent =
            new(File.Exists(Path.Combine(analyzer.BinDir, "SkAnalyzer.jar")));

        BindableReactiveProperty<bool> isSkriptPresent = new(File.Exists(Path.Combine(analyzer.BinDir, "Skript.jar")));

        BindableReactiveProperty<bool> isEverythingReady = isJavaValid.CombineLatest(isAnalyzerPresent,
                isSkriptPresent,
                (j, a, s) => j && a && s)
            .ToBindableReactiveProperty();

        isEverythingReady.SubscribeAwait(async (v, _) =>
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

        BindableReactiveProperty<TranslatableString> everythingTitle = isEverythingReady
            .Select(GetComponentStatusTitle)
            .ToBindableReactiveProperty(GetComponentStatusTitle(isEverythingReady.Value));

        SettingSection section = SettingSection.Create("Analyzer", TranslationKey.SettingsSectionAnalyzer)
            .WithIcon(Symbol.DocumentSearch)
            .WithSettings(
            [
                InfoSetting
                    .Create(TranslationKey.Attention)
                    .WithDescription(TranslationKey.SettingsAnalyzerWarning)
                    .WithIcon(Symbol.Warning)
                    .WithType(InfoSetting.InfoType.Warning),

                ExpandableSetting
                    .Create(everythingTitle)
                    .WithStatus(isEverythingReady)
                    .WithChildren(
                        GetJavaSetting(isJavaValid),
                        GetSkAnalyzerSetting(isAnalyzerPresent),
                        GetSkriptSetting(isSkriptPresent)
                    ),

                ExpandableSetting
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
                                    await analyzer.Initialize();
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
                    ),

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

    private static TranslatableString GetComponentStatusTitle(bool ready)
    {
        return ready
            ? TranslationKey.SettingsAnalyzerComponentsInstalled
            : TranslationKey.SettingsAnalyzerComponentsMissing;
    }

    private CompoundSetting GetJavaSetting(BindableReactiveProperty<bool> isJavaValid)
    {
        BindableReactiveProperty<TranslatableString?> description = isJavaValid
            .Select(v => v
                ? null
                : new TranslatableString(TranslationKey.SettingsAnalyzerJavaNotFound))
            .ToBindableReactiveProperty();

        return CompoundSetting
            .Create(TranslationKey.SettingsAnalyzerJavaTitle)
            .WithDescription(description)
            .WithChildren(
                ActionSetting.Create(TranslationKey.ButtonDownload)
                    .AsExternal()
                    .WithButtonVisible(isJavaValid.Select(v => !v).ToBindableReactiveProperty())
                    .WithAction(TranslationKey.ButtonDownload,
                        () => { links.OpenLink("https://adoptium.net/"); }),
                ActionSetting.Create(TranslationKey.ButtonCheck)
                    .WithButtonVisible(isJavaValid.Select(v => !v).ToBindableReactiveProperty())
                    .WithAction(TranslationKey.ButtonCheck, async () => { isJavaValid.Value = await CheckJava25(); })
            )
            .WithStatus(isJavaValid);
    }

    private CompoundSetting GetSkAnalyzerSetting(BindableReactiveProperty<bool> isAnalyzerPresent)
    {
        BindableReactiveProperty<TranslatableString?> description = isAnalyzerPresent
            .Select(v => v
                ? null
                : new TranslatableString(TranslationKey.SettingsAnalyzerSkAnalyzerMissing))
            .ToBindableReactiveProperty();

        return CompoundSetting
            .Create(TranslationKey.SettingsAnalyzerSkAnalyzerTitle)
            .WithDescription(description)
            .WithChildren(
                ActionSetting.Create(TranslationKey.ButtonDownload)
                    .AsExternal()
                    .WithButtonVisible(isAnalyzerPresent.Select(v => !v).ToBindableReactiveProperty())
                    .WithAction(TranslationKey.ButtonDownload,
                        () =>
                        {
                            links.OpenLink("https://github.com/SkEditorTeam/SkAnalyzer/releases/tag/v2.0.0-beta.7");
                        }),
                ActionSetting.Create(TranslationKey.ButtonSelectFile)
                    .WithButtonVisible(isAnalyzerPresent.Select(v => !v).ToBindableReactiveProperty())
                    .WithAction(TranslationKey.ButtonSelect, async () =>
                    {
                        string? jarFile = await SelectJarFile();
                        if (jarFile is null)
                        {
                            return;
                        }

                        Directory.CreateDirectory(analyzer.BinDir);
                        File.Move(jarFile, Path.Combine(analyzer.BinDir, "SkAnalyzer.jar"));
                        isAnalyzerPresent.Value = true;
                    })
            )
            .WithStatus(isAnalyzerPresent);
    }

    private CompoundSetting GetSkriptSetting(BindableReactiveProperty<bool> isSkriptPresent)
    {
        BindableReactiveProperty<TranslatableString?> description = isSkriptPresent
            .Select(v => v
                ? null
                : new TranslatableString(TranslationKey.SettingsAnalyzerSkriptMissing))
            .ToBindableReactiveProperty();

        return CompoundSetting
            .Create(TranslationKey.SettingsAnalyzerSkriptTitle)
            .WithDescription(description)
            .WithChildren(
                ActionSetting.Create(TranslationKey.ButtonDownload)
                    .AsExternal()
                    .WithButtonVisible(isSkriptPresent.Select(v => !v).ToBindableReactiveProperty())
                    .WithAction(TranslationKey.ButtonDownload,
                        () => { links.OpenLink("https://github.com/SkEditorTeam/Skratches/releases/latest"); }),
                ActionSetting.Create(TranslationKey.ButtonSelectFile)
                    .WithButtonVisible(isSkriptPresent.Select(v => !v).ToBindableReactiveProperty())
                    .WithAction(TranslationKey.ButtonSelect, async () =>
                    {
                        string? jarFile = await SelectJarFile();
                        if (jarFile is null)
                        {
                            return;
                        }

                        Directory.CreateDirectory(analyzer.BinDir);
                        File.Move(jarFile, Path.Combine(analyzer.BinDir, "Skript.jar"));
                        isSkriptPresent.Value = true;
                    })
            )
            .WithStatus(isSkriptPresent);
    }

    private async Task<string?> SelectJarFile()
    {
        string[]? paths = await filePicker.PickFilesAsync();
        if (paths is null)
        {
            return null;
        }

        if (paths.Length is not (0 or > 1) && paths[0].EndsWith(".jar"))
        {
            return paths[0];
        }

        Notification notification = Notification
            .Create(TranslationKey.SettingsAnalyzerInvalidFileSelectedTitle)
            .WithMessage(TranslationKey.SettingsAnalyzerInvalidFileSelectedMessage)
            .WithType(NotificationType.Error);

        notifs.Show(notification);
        return null;
    }

    private static async Task<bool> CheckJava25()
    {
        RefreshPath();

        Process? process = null;
        try
        {
            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return output.Contains(" 25") || output.Contains("25.");
        }
        catch
        {
            return false;
        }
        finally
        {
            process?.Dispose();
        }
    }

    private static void RefreshPath()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            string? machinePath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            string? userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

            string combinedPath = string.Join(Path.PathSeparator,
                new[] { machinePath, userPath }.Where(p => !string.IsNullOrWhiteSpace(p)));

            Environment.SetEnvironmentVariable("PATH", combinedPath);
        }
        catch
        {
            // ignored
        }
    }
}