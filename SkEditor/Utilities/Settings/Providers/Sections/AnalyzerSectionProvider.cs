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
    INotificationService notifs) : ISettingsSectionProvider
{
    public string ParentId => "Editor";

    public SettingSection GetSection()
    {
        string pluginsDir = Path.Combine(analyzer.WorkingDir, "plugins");

        BindableReactiveProperty<bool> isJavaValid = new(CheckJava25());

        BindableReactiveProperty<bool> isAnalyzerPresent =
            new(File.Exists(Path.Combine(analyzer.BinDir, "SkAnalyzer.jar")));

        BindableReactiveProperty<bool> isSkriptPresent = new(File.Exists(Path.Combine(analyzer.BinDir, "Skript.jar")));

        BindableReactiveProperty<bool> isEverythingReady = isJavaValid.CombineLatest(isAnalyzerPresent,
                isSkriptPresent,
                (j, a, s) => j && a && s)
            .ToBindableReactiveProperty();

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
                    .WithDescription("Manage Skript addons and other server plugins.".ToTranslatable())
                    .WithIcon(Symbol.Wrench)
                    .WithChildren(
                        ActionSetting
                            .Create("Server plugins folder".ToTranslatable())
                            .WithAction(TranslationKey.ButtonOpenFolder, () =>
                            {
                                Directory.CreateDirectory(pluginsDir);
                                links.OpenFolder(pluginsDir);
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

    private ActionSetting GetJavaSetting(BindableReactiveProperty<bool> isJavaValid)
    {
        BindableReactiveProperty<TranslatableString?> description = isJavaValid
            .Select(v => v
                ? null
                : "Java 25 was not found in PATH. SkAnalyzer requires JRE 25 to run.".ToTranslatable())
            .ToBindableReactiveProperty();

        return ActionSetting
            .Create("Java 25 runtime".ToTranslatable())
            .WithDescription(description)
            .AsExternal()
            .WithAction("Download".ToTranslatable(),
                () => { links.OpenLink("https://adoptium.net/"); })
            .WithButtonVisible(isJavaValid.Select(v => !v).ToBindableReactiveProperty())
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