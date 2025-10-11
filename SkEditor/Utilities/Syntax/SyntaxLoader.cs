using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AvaloniaEdit.Highlighting;
using FluentIcons.Common;
using Newtonsoft.Json;
using SkEditor.API;
using SkEditor.Utilities.Files;
using MarketplaceWindow = SkEditor.Views.Windows.Marketplace.MarketplaceWindow;
using Path = System.IO.Path;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Utilities.Syntax;

public class SyntaxLoader
{
    private static bool _enableChecking = true;
    private static string SyntaxFolder { get; } = Path.Combine(AppConfig.AppDataFolderPath, "Syntax Highlighting");

    public static List<FileSyntax> FileSyntaxes { get; set; } = [];

    // Sorted by extensions
    public static Dictionary<string, List<FileSyntax>> SortedFileSyntaxes { get; set; } = [];

    public static async Task LoadAdvancedSyntaxes()
    {
        Directory.CreateDirectory(SyntaxFolder);

        List<string> directories = Directory.GetDirectories(SyntaxFolder).ToList();
        bool hasOtherSyntaxDir = directories.Any(x => Path.GetFileName(x) == "Other Languages");

        if (!hasOtherSyntaxDir)
        {
            foreach (string directory in directories)
            {
                try
                {
                    FileSyntax syntax = await FileSyntax.LoadSyntax(directory);
                    if (syntax.Config == null || syntax.Config.Extensions.Length == 0)
                    {
                        return;
                    }

                    RegisterSyntax(syntax);
                }
                catch (Exception e)
                {
                    await SkEditorAPI.Windows.ShowDialog("Error",
                        $"Failed to load syntax {directory}\n\n{e.Message}\n{e.StackTrace}",
                        new SymbolIconSource { Symbol = Symbol.Important, IconVariant = IconVariant.Filled });
                }
            }
        }

        RefreshAllOpenedEditors();
    }

    private static void RegisterSyntax(FileSyntax syntax)
    {
        if (syntax.Config == null)
        {
            return;
        }

        FileSyntaxes.Add(syntax);
        foreach (string extension in syntax.Config.Extensions)
        {
            if (SortedFileSyntaxes.TryGetValue(extension, out List<FileSyntax>? value))
            {
                value.Add(syntax);
            }
            else
            {
                SortedFileSyntaxes.Add(extension, [syntax]);
            }
        }
    }

    public static void CheckConfiguredFileSyntaxes()
    {
        FileSyntaxes
            .Where(s => s.Config != null && !string.IsNullOrEmpty(s.Config.LanguageName))
            .ToList()
            .ForEach(s =>
                SkEditorAPI.Core.GetAppConfig().FileSyntaxes.Add(s.Config!.LanguageName, s.Config.FullIdName));
    }

    public static async Task<FileSyntax> LoadSyntax(string folder)
    {
        FileSyntax fileSyntax = await FileSyntax.LoadSyntax(folder);
        if (fileSyntax.Config != null)
        {
            RegisterSyntax(fileSyntax);
        }

        return fileSyntax;
    }

    public static async Task<bool> UnloadSyntax(string folder)
    {
        FileSyntax fileSyntax = await FileSyntax.LoadSyntax(folder);
        if (fileSyntax.Config == null)
        {
            return false;
        }

        FileSyntaxes.RemoveAll(x => x.Config != null && x.Config.FullIdName.Equals(fileSyntax.Config.FullIdName));
        foreach (string extension in fileSyntax.Config.Extensions)
        {
            if (SortedFileSyntaxes.TryGetValue(extension, out List<FileSyntax>? value))
            {
                value.Remove(fileSyntax);
            }

            SkEditorAPI.Core.GetAppConfig().FileSyntaxes.Remove(fileSyntax.Config.LanguageName);
        }

        return true;
    }

    public static async Task SelectSyntax(FileSyntax syntax, bool refresh = true)
    {
        if (syntax.Config == null)
        {
            return;
        }

        SkEditorAPI.Core.GetAppConfig().FileSyntaxes[syntax.Config.LanguageName] = syntax.Config.FullIdName;
        if (refresh)
        {
            await RefreshSyntaxAsync();
        }
    }

    public static async Task SetDefaultSyntax()
    {
        if (!Directory.Exists(Path.Combine(SyntaxFolder, "Default")))
        {
            await SetupDefaultSyntax();
        }

        SkEditorAPI.Core.GetAppConfig().FileSyntaxes.Clear();
        CheckConfiguredFileSyntaxes();
    }

    public static FileSyntax? GetConfiguredSyntaxForLanguage(string language)
    {
        if (FileSyntaxes.Count == 0)
        {
            _ = SetupDefaultSyntax();
        }

        string? configuredSyntax = SkEditorAPI.Core.GetAppConfig().FileSyntaxes.GetValueOrDefault(language);
        return FileSyntaxes.FirstOrDefault(x => x?.Config != null && (configuredSyntax == null
            ? x.Config.LanguageName == language
            : x.Config.FullIdName == configuredSyntax), FileSyntaxes.FirstOrDefault());
    }

    public static async Task<FileSyntax?> GetDefaultSyntax()
    {
        if (FileSyntaxes.Count != 0 || await SetupDefaultSyntax())
        {
            return GetConfiguredSyntaxForLanguage("Skript");
        }

        _enableChecking = false;
        return null;
    }

    public static async Task<bool> SetupDefaultSyntax()
    {
        try
        {
            string defaultSyntaxPath = Path.Combine(SyntaxFolder, "Default");
            FileSyntax.FileSyntaxConfig config = FileSyntax.DefaultSkriptConfig;

            Directory.CreateDirectory(SyntaxFolder);
            Directory.CreateDirectory(defaultSyntaxPath);

            HttpClient client = new();
            string url = MarketplaceWindow.MarketplaceUrl + "SkEditorFiles/Default.xshd";
            HttpResponseMessage response = await client.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync(Path.Combine(defaultSyntaxPath, "syntax.xshd"), content);

            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            await File.WriteAllTextAsync(Path.Combine(defaultSyntaxPath, "config.json"), json);

            FileSyntax syntax = await FileSyntax.LoadSyntax(defaultSyntaxPath);
            if (syntax.Config != null)
            {
                FileSyntaxes.Add(syntax);
            }

            return true;
        }
        catch
        {
            await SkEditorAPI.Windows.ShowDialog(Translation.Get("Error"),
                Translation.Get("FailedToDownloadSyntax"),
                new SymbolIconSource { Symbol = Symbol.Important, IconVariant = IconVariant.Filled },
                primaryButtonText: "Ok");

            return false;
        }
    }

    public static async Task RefreshSyntaxAsync(string? extension = null)
    {
        if (!_enableChecking)
        {
            return;
        }

        FileSyntax? defaultSyntax = await GetDefaultSyntax();

        OpenedFile? file = SkEditorAPI.Files.GetCurrentOpenedFile();
        if (file?.Editor is null)
        {
            return;
        }

        if (extension == null)
        {
            extension = Path.GetExtension(file.Path);
            if (string.IsNullOrWhiteSpace(extension) || !SortedFileSyntaxes.ContainsKey(extension))
            {
                if (defaultSyntax == null || defaultSyntax.Highlighting == null)
                {
                    return;
                }

                file.Editor.SyntaxHighlighting = defaultSyntax.Highlighting;
                return;
            }
        }

        if (!SortedFileSyntaxes.TryGetValue(extension, out List<FileSyntax>? fileSyntax))
        {
            if (defaultSyntax == null || defaultSyntax.Highlighting == null)
            {
                return;
            }

            file.Editor.SyntaxHighlighting = defaultSyntax.Highlighting;
            return;
        }

        FileSyntax? syntax = fileSyntax.FirstOrDefault(x => x.Config != null &&
                                                            x.Config.FullIdName == SkEditorAPI.Core.GetAppConfig()
                                                                .FileSyntaxes.GetValueOrDefault(x.Config.LanguageName)
                                                            && x.Config.Extensions.Contains(extension));

        if (syntax == null && fileSyntax.Count > 0)
        {
            syntax = fileSyntax[0];
        }

        if (syntax == null)
        {
            if (defaultSyntax == null || defaultSyntax.Highlighting == null)
            {
                return;
            }

            file.Editor.SyntaxHighlighting = defaultSyntax.Highlighting;
            return;
        }

        syntax = await FileSyntax.LoadSyntax(syntax.FolderName);
        if (syntax.Highlighting != null)
        {
            file.Editor.SyntaxHighlighting = syntax.Highlighting;
        }
    }

    public static void RefreshAllOpenedEditors()
    {
        List<OpenedFile> openedFiles = SkEditorAPI.Files.GetOpenedEditors();

        foreach (OpenedFile file in openedFiles)
        {
            if (file.Editor == null || file.Path == null)
            {
                continue;
            }

            string ext = Path.GetExtension(file.Path.TrimEnd('*').ToLower());
            if (string.IsNullOrWhiteSpace(ext) || !SortedFileSyntaxes.ContainsKey(ext))
            {
                continue;
            }

            FileSyntax? syntax = SortedFileSyntaxes[ext].FirstOrDefault(x => x.Config != null &&
                                                                             x.Config.FullIdName == SkEditorAPI.Core
                                                                                 .GetAppConfig().FileSyntaxes
                                                                                 .GetValueOrDefault(
                                                                                     x.Config.LanguageName)
                                                                             && x.Config.Extensions.Contains(ext));

            if (syntax == null && SortedFileSyntaxes[ext].Count > 0)
            {
                syntax = SortedFileSyntaxes[ext][0];
            }

            if (syntax?.Highlighting == null)
            {
                continue;
            }

            file.Editor.SyntaxHighlighting = syntax.Highlighting;
        }
    }

    public static IHighlightingDefinition? GetCurrentSkriptHighlighting()
    {
        FileSyntax? syntax = GetConfiguredSyntaxForLanguage("Skript");
        return syntax?.Highlighting;
    }
}