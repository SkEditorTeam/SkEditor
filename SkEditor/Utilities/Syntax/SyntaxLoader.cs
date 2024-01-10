using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using SkEditor.Utilities.Styling;
using Formatting = System.Xml.Formatting;

namespace SkEditor.Utilities.Syntax;
public class SyntaxLoader
{
    private static string SyntaxFolder { get; set; } = Path.Combine(AppConfig.AppDataFolderPath, "Syntax Highlighting");

    public static List<FileSyntax> FileSyntaxes { get; set; } = [];
    // Sorted by extensions
    public static Dictionary<string, List<FileSyntax>> SortedFileSyntaxes { get; set; } = new();

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
                    if (syntax.Config.Extensions.Length == 0) 
                        return;
                
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
            var response = await ApiVault.Get().ShowMessageWithIcon("Syntax migration", "In this version of SkEditor, a new syntax highlighting file format has been introduced. Files in the old format need to be deleted.\nIf you have created your own highlighting and don't want to lose it, make a backup.\nTo continue (and delete the files) click OK", new SymbolIconSource() { Symbol = Symbol.ImportantFilled },
                primaryButton: true, closeButtonContent: "Open syntax folder");

            if (response == ContentDialogResult.Primary)
            {
                Directory.Delete(Path.Combine(SyntaxFolder, "Other Languages"), true);
                foreach (var file in Directory.GetFiles(SyntaxFolder))
                    File.Delete(file);
            
                await SetupDefaultSyntax();
            }
            else if (response == ContentDialogResult.Secondary)
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = SyntaxFolder,
                    UseShellExecute = true
                });
            }
        }
    }

    private static void RegisterSyntax(FileSyntax syntax)
    {
        FileSyntaxes.Add(syntax);
        foreach (string extension in syntax.Config.Extensions)
        {
            if (SortedFileSyntaxes.ContainsKey(extension))
                SortedFileSyntaxes[extension].Add(syntax);
            else SortedFileSyntaxes.Add(extension, [syntax]);
        }
    }

    public static void CheckConfiguredFileSyntaxes()
    {
        foreach (var fileSyntax in FileSyntaxes)
        { 
            if (!ApiVault.Get().GetAppConfig().FileSyntaxes.ContainsKey(fileSyntax.Config.LanguageName))
            {
                ApiVault.Get().GetAppConfig().FileSyntaxes.Add(fileSyntax.Config.LanguageName, fileSyntax.Config.FullIdName);
            }
        }
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
            if (SortedFileSyntaxes.ContainsKey(extension)) 
                SortedFileSyntaxes[extension].Remove(fileSyntax);

            ApiVault.Get().GetAppConfig().FileSyntaxes.Remove(fileSyntax.Config.LanguageName);
        }

        return true;
    }

    public static void SelectSyntax(FileSyntax syntax, bool refresh = true)
    {
        ApiVault.Get().GetAppConfig().FileSyntaxes[syntax.Config.LanguageName] = syntax.Config.FullIdName;
        if (refresh) 
            RefreshSyntax();
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

            var json = JsonConvert.SerializeObject(config);
            await File.WriteAllTextAsync(Path.Combine(defaultSyntaxPath, "config.json"), json);

            FileSyntaxes.Add(await FileSyntax.LoadSyntax(defaultSyntaxPath));
        }
        catch (Exception e)
        {
            await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Error"),
                Translation.Get("FailedToDownloadSyntax"), new SymbolIconSource() { Symbol = Symbol.ImportantFilled },
                primaryButton: false, closeButtonContent: "Ok");
        }
    }

    public static void RefreshSyntax(string? extension = null)
    {
        var editor = ApiVault.Get().GetTextEditor();

        if (extension == null)
        {
            var currentOpenedFile = 
                ApiVault.Get().GetTabView().TabItems.Cast<TabViewItem>().ToList().FirstOrDefault(ApiVault.Get().IsFile);
            if (currentOpenedFile == null)
            {
                return;
            }

            extension = Path.GetExtension(currentOpenedFile.Tag.ToString()).ToLower();
            if (string.IsNullOrWhiteSpace(extension) || !SortedFileSyntaxes.ContainsKey(extension))
            {
                editor.SyntaxHighlighting = null;
                return;
            }
        }
        
        if (!SortedFileSyntaxes.ContainsKey(extension))
        {
            editor.SyntaxHighlighting = null;
            return; // No syntax for this extension
        }

        var syntax = SortedFileSyntaxes[extension].FirstOrDefault(x => 
            x.Config.FullIdName == ApiVault.Get().GetAppConfig().FileSyntaxes.GetValueOrDefault(x.Config.LanguageName) 
            && x.Config.Extensions.Contains(extension));
        if (syntax == null && SortedFileSyntaxes[extension].Count > 0)
        {
            syntax = SortedFileSyntaxes[extension][0];
        }
        
        if (syntax == null)
        {
            editor.SyntaxHighlighting = null;
            return;
        }
        
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
        
            if (syntax == null)
            {
                continue;
            }
            
            var ed = tab.Content as TextEditor;
            ed.SyntaxHighlighting = syntax.Highlighting;
        }
    }

}
