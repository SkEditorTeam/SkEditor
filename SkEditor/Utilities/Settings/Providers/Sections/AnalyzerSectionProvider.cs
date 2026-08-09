using System.Diagnostics;
using System.IO;
using FluentIcons.Common;
using R3;
using SkEditor.Analyzing;
using SkEditor.Contracts.Services;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using ExperimentsProperty = SkEditor.Utilities.Settings.ExperimentsConfig.ExperimentsConfigProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class AnalyzerSectionProvider(
    AppSettings appSettings,
    ExperimentsConfig experiments,
    ILinkService links,
    IAnalyzer analyzer) : ISettingsSectionProvider
{
    public string ParentId => "Editor";

    public SettingSection GetSection()
    {
        string pluginsDir = Path.Combine(analyzer.WorkingDir, "plugins");

        bool isJavaValid = CheckJava25();
        bool isAnalyzerPresent = File.Exists(Path.Combine(analyzer.BinDir, "SkAnalyzer.jar"));
        bool isSkriptPresent = Directory.Exists(pluginsDir) &&
                               Directory.GetFiles(pluginsDir, "Skript-*-skratched.jar").Length > 0;

        bool isEverythingReady = isJavaValid && isAnalyzerPresent && isSkriptPresent;

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
                    .Create(isEverythingReady
                        ? "All required components are installed.".ToTranslatable()
                        : "Some required components are missing.".ToTranslatable())
                    .WithStatus(isEverythingReady)
                    .WithChildren(
                        InfoSetting
                            .Create("Java 25 runtime".ToTranslatable())
                            .WithDescription(isJavaValid
                                ? null
                                : "Java 25 was not found in PATH. SkAnalyzer requires Java 25 to run.".ToTranslatable())
                            .WithStatus(isJavaValid),
                        InfoSetting
                            .Create("SkAnalyzer".ToTranslatable())
                            .WithDescription(isAnalyzerPresent
                                ? null
                                : "SkAnalyzer.jar is missing.".ToTranslatable())
                            .WithStatus(isAnalyzerPresent),
                        InfoSetting
                            .Create("Skript".ToTranslatable())
                            .WithDescription(isSkriptPresent
                                ? null
                                : "Skript patched JAR is missing.".ToTranslatable())
                            .WithStatus(isSkriptPresent)

                        // ActionSetting
                        //     .Create("Setup".ToTranslatable())
                        //     .WithDescription(
                        //         "Download or update all the components.".ToTranslatable())
                        //     .WithAction(
                        //         isEverythingReady ? "Update".ToTranslatable() : "Download".ToTranslatable(),
                        //         async () =>
                        //         {
                        //             
                        //         })
                    ),

                ExpandableSetting
                    .Create("Tools".ToTranslatable())
                    .WithDescription("Manage Skript addons, server plugins, and Analyzer cache.".ToTranslatable())
                    .WithIcon(Symbol.Wrench)
                    .WithChildren(
                        ActionSetting
                            .Create("Server plugins folder".ToTranslatable())
                            .WithAction(TranslationKey.ButtonOpenFolder, () => links.OpenFolder(pluginsDir))

                        // ActionSetting
                        //     .Create("AOT cache".ToTranslatable())
                        //     .WithDescription("Regenerate Ahead-Of-Time compilation cache if startup errors occur.".ToTranslatable())
                        //     .WithIcon(Symbol.ArrowClockwise)
                        //     .WithAction("Rebuild".ToTranslatable(), () =>
                        //     {
                        //     })
                        //     .WithVisible(appSettings.ObservePropertyChanged(x => x.IsDevModeEnabled).ToBindableReactiveProperty())
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

    private static bool CheckJava25()
    {
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
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
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
}