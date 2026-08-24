using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Platform;
using SkEditor.Contracts.Services;
using SkEditor.Models.UI;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Services;

public class SyntaxService(IConfigService config, AppSettings settings, ILogService logs) : ISyntaxService
{
    private const string SyntaxAssetsDirectory = "avares://SkEditor/Assets/Syntax";
    private static readonly string[] DefaultGrammars = ["skript.tmLanguage.json"];
    private static readonly string[] DefaultThemes = ["dark_skript.json", "ethereal_skript.json"];
    private readonly Dictionary<string, string> _scopeToGrammarFile = [];

    private readonly List<SyntaxPair> _syntaxPairs = [];
    public string GrammarFolder => Path.Combine(SyntaxFolderPath, "Grammars");
    public string SyntaxFolderPath { get; set; } = Path.Combine(config.GetAppDataPath(), "Syntax");
    public string ThemeFolder => Path.Combine(SyntaxFolderPath, "Themes");

    public void LoadSyntaxes()
    {
        Directory.CreateDirectory(GrammarFolder);
        Directory.CreateDirectory(ThemeFolder);

        EnsureDefaultFiles(DefaultGrammars, GrammarFolder);
        EnsureDefaultFiles(DefaultThemes, ThemeFolder);

        if (settings.SyntaxPairs.Count == 0)
        {
            settings.SyntaxPairs.Add(new SyntaxPairDto([".sk", ""], "skript.tmLanguage.json", "dark_skript.json"));
        }

        _syntaxPairs.Clear();
        _scopeToGrammarFile.Clear();

        foreach (SyntaxPairDto dto in settings.SyntaxPairs)
        {
            string scope = ResolveScopeName(dto.Grammar);
            SyntaxGrammar grammar = new(dto.Grammar, scope);
            SyntaxTheme theme = new(dto.Theme);

            _syntaxPairs.Add(new SyntaxPair(dto.Extensions, grammar, theme));
            _scopeToGrammarFile[scope] = dto.Grammar;
        }
    }

    public SyntaxPair? GetPairByExtension(string? extension)
    {
        string normalizedExt = extension?.ToLowerInvariant() ?? "";
        return _syntaxPairs.FirstOrDefault(p => p.Extensions.Contains(normalizedExt));
    }

    public string? GetGrammarFileByScope(string scopeName)
    {
        return _scopeToGrammarFile.GetValueOrDefault(scopeName);
    }

    public Stream? GetGrammarStream(string fileName)
    {
        string localPath = Path.Combine(GrammarFolder, fileName);
        if (File.Exists(localPath))
        {
            return File.OpenRead(localPath);
        }

        Uri assetUri = new($"{SyntaxAssetsDirectory}/{fileName}");
        return AssetLoader.Exists(assetUri) ? AssetLoader.Open(assetUri) : null;
    }

    public Stream? GetThemeStream(string fileName)
    {
        string actualFileName = fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? fileName
            : $"{fileName}.json";

        string localPath = Path.Combine(ThemeFolder, actualFileName);
        if (File.Exists(localPath))
        {
            return File.OpenRead(localPath);
        }

        Uri assetUri = new($"{SyntaxAssetsDirectory}/{actualFileName}");
        return AssetLoader.Exists(assetUri) ? AssetLoader.Open(assetUri) : null;
    }

    public List<string> GetAvailableGrammarFiles()
    {
        return Directory.EnumerateFiles(GrammarFolder, "*.json").Select(Path.GetFileName).ToList()!;
    }

    public List<string> GetAvailableThemeFiles()
    {
        return Directory.EnumerateFiles(ThemeFolder, "*.json").Select(Path.GetFileName).ToList()!;
    }

    private string ResolveScopeName(string grammarFileName)
    {
        try
        {
            using Stream? stream = GetGrammarStream(grammarFileName);
            if (stream == null)
            {
                return "source.unknown";
            }

            using JsonDocument doc = JsonDocument.Parse(stream);
            if (doc.RootElement.TryGetProperty("scopeName", out JsonElement scopeProp))
            {
                return scopeProp.GetString() ?? "source.unknown";
            }
        }
        catch (Exception ex)
        {
            logs.Error(ex, $"Failed to parse scopeName from {grammarFileName}");
        }

        return "source.unknown";
    }

    private static void EnsureDefaultFiles(string[] files, string destinationFolder)
    {
        foreach (string file in files)
        {
            string targetPath = Path.Combine(destinationFolder, file);
            if (File.Exists(targetPath))
            {
                continue;
            }

            Uri assetUri = new($"{SyntaxAssetsDirectory}/{file}");
            if (!AssetLoader.Exists(assetUri))
            {
                continue;
            }

            using Stream assetStream = AssetLoader.Open(assetUri);
            using FileStream fileStream = File.Create(targetPath);
            assetStream.CopyTo(fileStream);
        }
    }
}