using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace SkEditor.Utilities.Syntax;
public class SyntaxLoader
{
    private static string SyntaxFolder { get; set; } = Path.Combine(AppConfig.AppDataFolderPath, "Syntax Highlighting");

    public static List<FileSyntax> FileSyntaxes { get; set; } = [];
    // Sorted by extensions
    public static Dictionary<string, List<FileSyntax>> SortedFileSyntaxes { get; set; } = [];

    public static async void LoadAdvancedSyntaxes()
    {
        Directory.CreateDirectory(SyntaxFolder);

        var directories = Directory.GetDirectories(SyntaxFolder).ToList();
        var hasOtherSyntaxDir = directories.Any(x => Path.GetFileName(x) == "Other Languages");

        if (!hasOtherSyntaxDir)
        {
            foreach (var directory in directories)
            {
                try
                {
                    FileSyntax syntax = await FileSyntax.LoadSyntax(directory);
                    if (syntax.Config.Extensions.Length == 0) return;

                    RegisterSyntax(syntax);
                }
                catch (Exception e)
                {
                    await ApiVault.Get().ShowMessageWithIcon("Error", $"Failed to load syntax {directory}\n\n{e.Message}\n{e.StackTrace}", new SymbolIconSource() { Symbol = Symbol.ImportantFilled },
                        primaryButton: false);
                }
            }
        }
        else
        {
            var response = await ApiVault.Get().ShowMessageWithIcon("Syntax migration", "In this version of SkEditor, a new syntax highlighting file format has been introduced. Files in the old format need to be deleted.\nIf you have created your own highlighting and don't want to lose it, make a backup.\nTo continue (and delete the files) click Confirm.", new SymbolIconSource() { Symbol = Symbol.ImportantFilled },
                primaryButton: true, closeButtonContent: "Open syntax folder");

            if (response == ContentDialogResult.Primary)
            {
                Directory.Delete(Path.Combine(SyntaxFolder, "Other Languages"), true);
                Directory.GetFiles(SyntaxFolder).ToList().ForEach(File.Delete);

                await SetupDefaultSyntax();
            }
            else if (response == ContentDialogResult.None)
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = SyntaxFolder,
                    UseShellExecute = true
                });
                Environment.Exit(0);
            }
        }

        await RefreshSyntaxAsync();
    }

    private static void RegisterSyntax(FileSyntax syntax)
    {
        FileSyntaxes.Add(syntax);
        foreach (string extension in syntax.Config.Extensions)
        {
            if (SortedFileSyntaxes.TryGetValue(extension, out List<FileSyntax>? value)) value.Add(syntax);
            else SortedFileSyntaxes.Add(extension, [syntax]);
        }
    }

    public static void CheckConfiguredFileSyntaxes()
    {
        FileSyntaxes
            .Where(s => !ApiVault.Get().GetAppConfig().FileSyntaxes.ContainsKey(s.Config.LanguageName))
            .ToList()
            .ForEach(s => ApiVault.Get().GetAppConfig().FileSyntaxes.Add(s.Config.LanguageName, s.Config.FullIdName));
    }

    public static async Task<FileSyntax> LoadSyntax(string folder)
    {
        var fileSyntax = await FileSyntax.LoadSyntax(folder);
        RegisterSyntax(fileSyntax);
        return fileSyntax;
    }

    public static async Task<bool> UnloadSyntax(string folder)
    {
        var fileSyntax = await FileSyntax.LoadSyntax(folder);
        FileSyntaxes.RemoveAll(x => x.Config.FullIdName.Equals(fileSyntax.Config.FullIdName));
        foreach (string extension in fileSyntax.Config.Extensions)
        {
            if (SortedFileSyntaxes.TryGetValue(extension, out List<FileSyntax>? value))
                value.Remove(fileSyntax);

            ApiVault.Get().GetAppConfig().FileSyntaxes.Remove(fileSyntax.Config.LanguageName);
        }

        return true;
    }

    public static void SelectSyntax(FileSyntax syntax, bool refresh = true)
    {
        ApiVault.Get().GetAppConfig().FileSyntaxes[syntax.Config.LanguageName] = syntax.Config.FullIdName;
        if (refresh) RefreshSyntaxAsync();
    }

    public static async void SetDefaultSyntax()
    {
        if (!Directory.Exists(Path.Combine(SyntaxFolder, "Default")))
        {
            await SetupDefaultSyntax();
        }

        ApiVault.Get().GetAppConfig().FileSyntaxes.Clear();
        CheckConfiguredFileSyntaxes();
    }

    public static FileSyntax GetConfiguredSyntaxForLanguage(string language)
    {
        var configuredSyntax = ApiVault.Get().GetAppConfig().FileSyntaxes.GetValueOrDefault(language);
        return FileSyntaxes.FirstOrDefault(x => configuredSyntax == null
            ? x.Config.LanguageName == language
            : x.Config.FullIdName == configuredSyntax) ?? FileSyntaxes[0];
    }

    public static async Task<FileSyntax> GetDefaultSyntax()
    {
        if (FileSyntaxes.Count == 0)
        {
            await SetupDefaultSyntax();
        }

        return GetConfiguredSyntaxForLanguage("Skript");
    }

    public static async Task SetupDefaultSyntax()
    {
        try
        {
            var defaultSyntaxPath = Path.Combine(SyntaxFolder, "Default");
            var config = FileSyntax.DefaultSkriptConfig;

            Directory.CreateDirectory(SyntaxFolder);
            Directory.CreateDirectory(defaultSyntaxPath);

            HttpClient client = new();
            string url = "https://marketplace-skeditor.vercel.app/SkEditorFiles/Default.xshd";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync(Path.Combine(defaultSyntaxPath, "syntax.xshd"), content);

            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            await File.WriteAllTextAsync(Path.Combine(defaultSyntaxPath, "config.json"), json);

            FileSyntaxes.Add(await FileSyntax.LoadSyntax(defaultSyntaxPath));
        }
        catch
        {
            await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Error"),
                Translation.Get("FailedToDownloadSyntax"), new SymbolIconSource() { Symbol = Symbol.ImportantFilled },
                primaryButton: false, closeButtonContent: "Ok");
        }
    }

    public static async Task RefreshSyntaxAsync(string? extension = null)
    {
        var defaultSyntax = await GetDefaultSyntax();
        var editor = ApiVault.Get().GetTextEditor();
        if (editor == null)
            return;

        if (extension == null)
        {
            extension = Path.GetExtension((ApiVault.Get().GetTabView().SelectedItem as TabViewItem).Tag.ToString());
            if (string.IsNullOrWhiteSpace(extension) || !SortedFileSyntaxes.ContainsKey(extension))
            {
                editor.SyntaxHighlighting = defaultSyntax.Highlighting;
                return;
            }
        }

        if (!SortedFileSyntaxes.TryGetValue(extension, out List<FileSyntax>? fileSyntax))
        {
            editor.SyntaxHighlighting = defaultSyntax.Highlighting;
            return;
        }

        var syntax = fileSyntax.FirstOrDefault(x =>
            x.Config.FullIdName == ApiVault.Get().GetAppConfig().FileSyntaxes.GetValueOrDefault(x.Config.LanguageName)
            && x.Config.Extensions.Contains(extension));
        if (syntax == null && fileSyntax.Count > 0)
        {
            syntax = fileSyntax[0];
        }

        if (syntax == null)
        {
            editor.SyntaxHighlighting = defaultSyntax.Highlighting;
            return;
        }

        syntax = await FileSyntax.LoadSyntax(syntax.FolderName);
        editor.SyntaxHighlighting = syntax.Highlighting;
    }

    public static void RefreshAllOpenedEditors()
    {
        var tabs = ApiVault.Get().GetTabView().TabItems
            .OfType<TabViewItem>()
            .Where(tab => tab.Content is TextEditor)
            .ToList();

        foreach (var tab in tabs)
        {
            var ext = Path.GetExtension(tab.Tag?.ToString()?.ToLower() ?? "");
            if (string.IsNullOrWhiteSpace(ext) || !SortedFileSyntaxes.ContainsKey(ext))
            {
                continue;
            }

            var syntax = SortedFileSyntaxes[ext].FirstOrDefault(x =>
                x.Config.FullIdName == ApiVault.Get().GetAppConfig().FileSyntaxes.GetValueOrDefault(x.Config.LanguageName)
                && x.Config.Extensions.Contains(ext));
            if (syntax == null && SortedFileSyntaxes[ext].Count > 0)
            {
                syntax = SortedFileSyntaxes[ext][0];
            }

            if (syntax == null) continue;

            var editor = tab.Content as TextEditor;
            editor.SyntaxHighlighting = syntax.Highlighting;
        }
    }

}
