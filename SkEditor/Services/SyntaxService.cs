using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Platform;
using SkEditor.Contracts.Services;
using SkEditor.Models.UI;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Services;

public class SyntaxService(IConfigService config, AppSettings settings, ILogService logs) : ISyntaxService
{
    public string SyntaxFolderPath { get; set; } = Path.Combine(config.GetAppDataPath(), "Syntax");
    private string GrammarFolder => Path.Combine(SyntaxFolderPath, "Grammars");
    private string ThemeFolder => Path.Combine(SyntaxFolderPath, "Themes");

    private const string SyntaxAssetsDirectory = "avares://SkEditor/Assets/Syntax";
    private static readonly string[] DefaultGrammars = ["skript.tmLanguage.json"];
    private static readonly string[] DefaultThemes = ["dark_skript.json", "ethereal_skript.json"];

    private List<SyntaxPair> _syntaxPairs = [];

    public void LoadSyntaxes()
    {
        Directory.CreateDirectory(GrammarFolder);
        Directory.CreateDirectory(ThemeFolder);

        List<string> grammarFiles = Directory.EnumerateFiles(GrammarFolder, "*.json").ToList();
        List<string> themeFiles = Directory.EnumerateFiles(ThemeFolder, "*.json").ToList();

        if (grammarFiles.Count == 0)
        {
            CopyDefaultFiles(DefaultGrammars, GrammarFolder);
        }

        if (themeFiles.Count == 0)
        {
            CopyDefaultFiles(DefaultThemes, ThemeFolder);
        }

        if (settings.SyntaxPairs.Count == 0)
        {
            SyntaxPairDto pairDto = new([".sk"], "skript", "dark_skript");
            settings.SyntaxPairs.Add(pairDto);
        }

        _syntaxPairs = settings.SyntaxPairs
            .Select(p =>
            {
                SyntaxGrammar grammar = new(p.Grammar);
                SyntaxTheme theme = new(p.Theme);
                return new SyntaxPair(p.Extensions, grammar, theme);
            })
            .ToList();
    }

    public SyntaxPair? GetPairByExtension(string? extension)
    {
        return _syntaxPairs.FirstOrDefault(p => p.Extensions.Contains(extension));
    }

    public Stream GetThemeStream()
    {
    }

    private static void CopyDefaultFiles(string[] files, string destination)
    {
        foreach (string file in files)
        {
            string path = Path.Combine(SyntaxAssetsDirectory, file);
            using Stream stream = AssetLoader.Open(new Uri(path));
            using FileStream fileStream = File.Create(Path.Combine(destination, file));
            stream.CopyTo(fileStream);
        }
    }
}