using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Platform;
using SkEditor.Contracts.Services;
using SkEditor.Models.UI;
using SkEditor.Utilities.Settings.Configs;
using TextMateSharp.Grammars;

namespace SkEditor.Services;

public partial class SyntaxService(IConfigService config, AppSettings settings, ILogService logs) : ISyntaxService
{
    private const string SyntaxAssetsDirectory = "avares://SkEditor/Assets/Syntax";
    private static readonly string[] DefaultGrammars = ["skript.tmLanguage.json"];
    private static readonly string[] DefaultThemes = ["dark_skript.json", "ethereal_skript.json"];

    private static readonly ThemeName[] BuiltInTextmateThemes =
        [ThemeName.DarkPlus, ThemeName.OneDark, ThemeName.Monokai, ThemeName.Dracula];

    private readonly Dictionary<string, string> _scopeToGrammarFile = [];

    private readonly List<SyntaxPair> _syntaxPairs = [];
    public string GrammarFolder => Path.Combine(SyntaxFolderPath, "Grammars");
    public string SyntaxFolderPath { get; set; } = Path.Combine(config.GetAppDataPath(), "Syntax");
    public string ThemeFolder => Path.Combine(SyntaxFolderPath, "Themes");

    public List<SyntaxTheme> AvailableThemes { get; private set; } = [];
    public List<SyntaxGrammar> AvailableGrammars { get; private set; } = [];

    public event Action? SyntaxesReloaded;

    public void LoadSyntaxes()
    {
        Directory.CreateDirectory(GrammarFolder);
        Directory.CreateDirectory(ThemeFolder);

        CopyDefaultFiles(DefaultGrammars, GrammarFolder);
        CopyDefaultFiles(DefaultThemes, ThemeFolder);

        if (settings.SyntaxPairs.Count == 0)
        {
            settings.SyntaxPairs.Add(new SyntaxPairDto([".sk", ""], "skript.tmLanguage.json", "dark_skript.json"));
            settings.SyntaxPairs.Add(new SyntaxPairDto(["*"], null, "DarkPlus"));
        }

        AvailableThemes = Directory.EnumerateFiles(ThemeFolder, "*.json")
            .Select(file =>
            {
                string fileName = Path.GetFileName(file);
                (string? name, string? language) = ReadPropertiesFromTheme(file);
                return new SyntaxTheme(fileName, name, language);
            })
            .Concat(BuiltInTextmateThemes.Select(tn =>
                new SyntaxTheme(tn.ToString(), ToSentenceCase(tn.ToString()))
            ))
            .ToList();

        AvailableGrammars = Directory.EnumerateFiles(GrammarFolder, "*.json")
            .Select(file =>
            {
                string fileName = Path.GetFileName(file);
                string scope = ResolveScopeName(fileName);
                return new SyntaxGrammar(fileName, scope);
            })
            .ToList();

        SyntaxesReloaded?.Invoke();

        RefreshPairs();
    }

    public void RefreshPairs()
    {
        _syntaxPairs.Clear();
        _scopeToGrammarFile.Clear();

        foreach (SyntaxPairDto dto in settings.SyntaxPairs)
        {
            string? scope = dto.Grammar is null ? null : ResolveScopeName(dto.Grammar);
            SyntaxGrammar? grammar = AvailableGrammars.FirstOrDefault(g => g.ScopeName == scope);
            SyntaxTheme? theme =
                AvailableThemes.FirstOrDefault(t => t.FileName.Equals(dto.Theme, StringComparison.OrdinalIgnoreCase));

            if (dto.Grammar is not null && grammar is null)
            {
                grammar = AvailableGrammars.FirstOrDefault(g => g.FileName == DefaultGrammars[0])
                          ?? AvailableGrammars.FirstOrDefault();

                if (grammar is not null)
                {
                    dto.Grammar = grammar.FileName;
                    scope = grammar.ScopeName;
                }
            }
            
            if (theme is null)
            {
                theme = AvailableThemes.FirstOrDefault(t => t.FileName == DefaultThemes[0])
                        ?? AvailableThemes.FirstOrDefault();

                if (theme is not null)
                {
                    dto.Theme = theme.FileName;
                }
            }

            if (theme is null)
            {
                continue;
            }

            _syntaxPairs.Add(new SyntaxPair(dto.Extensions, grammar, theme));
            if (scope is not null && dto.Grammar is not null)
            {
                _scopeToGrammarFile[scope] = dto.Grammar;
            }
        }
    }

    public SyntaxPair? GetPairByExtension(string? extension)
    {
        string normalizedExt = extension?.ToLowerInvariant() ?? "";

        SyntaxPair? exactMatch = _syntaxPairs.FirstOrDefault(p => p.Extensions.Contains(normalizedExt));
        return exactMatch ??
               _syntaxPairs.FirstOrDefault(p => p.Extensions.Contains("*") || p.Extensions.Contains(".*"));
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

    private static void CopyDefaultFiles(string[] files, string destinationFolder)
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

    private static (string? name, string? language) ReadPropertiesFromTheme(string filePath)
    {
        try
        {
            using Stream stream = File.OpenRead(filePath);
            using JsonDocument doc = JsonDocument.Parse(stream);

            string? name = doc.RootElement.GetProperty("name").GetString();
            string? language = doc.RootElement.GetProperty("language").GetString();
            return (name, language);
        }
        catch
        {
            // ignored
        }

        return default;
    }

    private static string ToSentenceCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        string result = PascalCaseBoundaryRegex().Replace(value, " $1").ToLowerInvariant();

        return char.ToUpperInvariant(result[0]) + result[1..];
    }

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex PascalCaseBoundaryRegex();
}