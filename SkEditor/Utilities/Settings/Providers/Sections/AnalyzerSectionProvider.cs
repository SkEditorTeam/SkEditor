using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FluentIcons.Common;
using R3;
using SkEditor.Analyzing;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Notifications;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using ExperimentsProperty = SkEditor.Utilities.Settings.ExperimentsConfig.ExperimentsConfigProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

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
                    .Create("Analyzer failed to load".ToTranslatable())
                    .WithMessage(
                        "Something went wrong while starting the Analyzer. Take a look at the logs for more information."
                            .ToTranslatable())
                    .WithType(NotificationType.Error);
                notifs.Show(notification);

                logs.Error(ex, "Failed to initialize Analyzer process.");
            }
        }, AwaitOperation.Drop);

        BindableReactiveProperty<TranslatableString> everythingTitle = isEverythingReady
            .Select(v => v
                ? "All required components are installed.".ToTranslatable()
                : "Some required components are missing.".ToTranslatable())
            .ToBindableReactiveProperty(
                isEverythingReady.Value
                    ? "All required components are installed.".ToTranslatable()
                    : "Some required components are missing.".ToTranslatable()
            );

        SettingSection section = SettingSection.Create("Analyzer", "Analyzer".ToTranslatable())
            .WithIcon(Symbol.DocumentSearch)
            .WithSettings(
            [
                InfoSetting
                    .Create(TranslationKey.Attention)
                    .WithDescription(
                        "The Analyzer is an experimental feature and can be toggled in the Experiments section."
                            .ToTranslatable())
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
                    .Create("Tools".ToTranslatable())
                    .WithDescription("Manage Skript addons and analyzer process state.".ToTranslatable())
                    .WithIcon(Symbol.Wrench)
                    .WithChildren(
                        ActionSetting
                            .Create("Server plugins folder".ToTranslatable())
                            .WithAction(TranslationKey.ButtonOpenFolder, () =>
                            {
                                Directory.CreateDirectory(pluginsDir);
                                links.OpenFolder(pluginsDir);
                            }),
                        ActionSetting
                            .Create("Restart".ToTranslatable())
                            .WithDescription(
                                "Restarts the Analyzer process if it hanged, crashed, or you're just feeling like it."
                                    .ToTranslatable())
                            .WithAction("Restart".ToTranslatable(), async () =>
                            {
                                try
                                {
                                    await analyzer.Initialize();
                                }
                                catch (Exception ex)
                                {
                                    Notification notification = Notification
                                        .Create("Restart failed".ToTranslatable())
                                        .WithMessage(
                                            "Something went wrong while restarting the Analyzer. Take a look at the logs for more information."
                                                .ToTranslatable())
                                        .WithType(NotificationType.Error);
                                    notifs.Show(notification);

                                    logs.Error(ex, "Failed to restart the Analyzer process.");
                                }
                            }),
                        ActionSetting
                            .Create("Clear AOT cache".ToTranslatable())
                            .WithDescription(
                                "Deletes the generated .aot cache file. It will be recreated on the next startup."
                                    .ToTranslatable())
                            .WithAction("Clear".ToTranslatable(), () =>
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
                                        .Create("AOT cache cleared".ToTranslatable())
                                        .WithMessage(
                                            "AOT cache file deleted. Restart the Analyzer to regenerate it."
                                                .ToTranslatable())
                                        .WithType(NotificationType.Success);
                                    notifs.Show(notification);
                                }
                                catch (Exception ex)
                                {
                                    Notification notification = Notification
                                        .Create("Error".ToTranslatable())
                                        .WithMessage($"Failed to delete AOT cache: {ex.Message}".ToTranslatable())
                                        .WithType(NotificationType.Error);
                                    notifs.Show(notification);
                                }
                            })
                    ),

                ToggleSetting
                    .Create("Scrollbar error markers".ToTranslatable())
                    .WithDescription("Displays colored error and warning indicators directly on the editor scrollbar."
                        .ToTranslatable())
                    .WithIcon(Symbol.SidebarSearch)
                    .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.AnalyzerErrorScrollMarkersEnabled)),

                ToggleSetting
                    .Create("Diagnostic tooltips".ToTranslatable())
                    .WithDescription("Shows detailed error tooltips when hovering over code issues.".ToTranslatable())
                    .WithIcon(Symbol.CommentNote)
                    .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.AnalyzerErrorTooltipsEnabled))
            ]);

        section.IsVisible = experiments
            .ObservePropertyChanged(x => x.AnalyzerEnabled)
            .ToBindableReactiveProperty();

        return section;
    }

    private CompoundSetting GetJavaSetting(BindableReactiveProperty<bool> isJavaValid)
    {
        BindableReactiveProperty<TranslatableString?> description = isJavaValid
            .Select(v => v
                ? null
                : "Java 25 was not found in PATH. SkAnalyzer requires JRE 25 to run.".ToTranslatable())
            .ToBindableReactiveProperty();

        return CompoundSetting
            .Create("Java 25 runtime".ToTranslatable())
            .WithDescription(description)
            .WithChildren(
                ActionSetting.Create("Download".ToTranslatable())
                    .AsExternal()
                    .WithButtonVisible(isJavaValid.Select(v => !v).ToBindableReactiveProperty())
                    .WithAction("Download".ToTranslatable(),
                        () => { links.OpenLink("https://adoptium.net/"); }),
                ActionSetting.Create("Check".ToTranslatable())
                    .WithButtonVisible(isJavaValid.Select(v => !v).ToBindableReactiveProperty())
                    .WithAction("Check".ToTranslatable(), async () => { isJavaValid.Value = await CheckJava25(); })
            )
            .WithStatus(isJavaValid);
    }

    private CompoundSetting GetSkAnalyzerSetting(BindableReactiveProperty<bool> isAnalyzerPresent)
    {
        BindableReactiveProperty<TranslatableString?> description = isAnalyzerPresent
            .Select(v => v
                ? null
                : "SkAnalyzer.jar is missing.".ToTranslatable())
            .ToBindableReactiveProperty();

        return CompoundSetting
            .Create("SkAnalyzer".ToTranslatable())
            .WithDescription(description)
            .WithChildren(
                ActionSetting.Create("Download".ToTranslatable())
                    .AsExternal()
                    .WithButtonVisible(isAnalyzerPresent.Select(v => !v).ToBindableReactiveProperty())
                    .WithAction("Download".ToTranslatable(),
                        () =>
                        {
                            links.OpenLink("https://github.com/SkEditorTeam/SkAnalyzer/releases/tag/v2.0.0-beta.7");
                        }),
                ActionSetting.Create("Select file".ToTranslatable())
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
                : "Skript patched JAR is missing.".ToTranslatable())
            .ToBindableReactiveProperty();

        return CompoundSetting
            .Create("Skript".ToTranslatable())
            .WithDescription(description)
            .WithChildren(
                ActionSetting.Create("Download".ToTranslatable())
                    .AsExternal()
                    .WithButtonVisible(isSkriptPresent.Select(v => !v).ToBindableReactiveProperty())
                    .WithAction("Download".ToTranslatable(),
                        () => { links.OpenLink("https://github.com/SkEditorTeam/Skratches/releases/latest"); }),
                ActionSetting.Create("Select file".ToTranslatable())
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
            .Create("Invalid file selected".ToTranslatable())
            .WithMessage("You need to select a single .jar file.".ToTranslatable())
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
        try
        {
            string? machinePath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            string? userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            string combinedPath = $"{machinePath};{userPath}";

            Environment.SetEnvironmentVariable("PATH", combinedPath);
        }
        catch
        {
            // ignored
        }
    }
}