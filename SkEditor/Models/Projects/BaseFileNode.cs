using System;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using SkEditor.API;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.Models.Projects;

public abstract class BaseFileNode : FileSystemNode
{
    protected BaseFileNode(string name, string path) : base(name, path, false)
    {
        string extension = System.IO.Path.GetExtension(name).ToLower();
        (Symbol icon, string color) = extension switch
        {
            ".sk" => (Symbol.Script, "#00C853"),
            ".sql" or ".db" or ".sqlite" or ".sqlite3" => (Symbol.Database, "#BEF26F"),
            ".csv" or ".tsv" => (Symbol.DocumentTable, "#FF8A30"),

            ".yml" or ".yaml" => (Symbol.DocumentYml, "#49B7E3"),
            ".json" or ".json5" or ".mcmeta" => (Symbol.Braces, "#FFB300"),
            ".toml" or ".properties" or ".ini" or ".cfg" or ".conf" or ".config" or ".props" or ".settings" => (
                Symbol.DocumentSettings,
                "#49B7E3"),
            ".xml" => (Symbol.Code, "#44D468"),

            ".schem" or ".schematic" or ".litematic" or ".structure" => (Symbol.DocumentCube, "#FF9800"),
            ".nbt" or ".dat" or ".mca" => (Symbol.DocumentCube, "#607D8B"),

            ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".webp" or ".svg"
                or ".ico" => (Symbol.Image, "#29B6F6"),
            ".mp3" or ".wav" or ".ogg" or ".flac" => (Symbol.MusicNote2, "#BCF02E"),
            ".mp4" or ".mkv" or ".avi" or ".mov" or ".ogv" or ".m4a" => (Symbol.Video, "#FF5252"),

            ".txt" or ".info" => (Symbol.DocumentText, "#CFD8DC"),
            ".log" => (Symbol.DocumentHeader, "#BDCFD9"),
            ".md" => (Symbol.DocumentBulletList, "#4FC3F7"),

            ".dll" or ".so" or ".bin" or ".sys" => (Symbol.DocumentCube, "#F53D3D"),
            ".exe" or ".msi" => (Symbol.Play, "#35D4B4"),
            ".jar" or ".java" => (Symbol.Box, "#F03765"),
            ".class" => (Symbol.DocumentCube, "#D32F2F"),

            ".bat" or ".cmd" or ".sh" or ".bash" or ".zsh" or ".ps1" => (Symbol.WindowConsole, "#4AB9ED"),

            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" or ".xz" => (Symbol.FolderZip,
                "#FF87B7"),

            ".env" or ".pem" or ".key" or ".pub" or ".cert" or ".ppk" => (Symbol.Key,
                "#E38B2D"),

            ".gitignore" or ".gitattributes" => (Symbol.BranchFork, "#F26A3D"),

            ".pdf" => (Symbol.DocumentPdf, "#FF595C"),
            ".doc" or ".docx" or ".odt" => (Symbol.DocumentText, "#5CC9FF"),
            ".pptx" or ".ppt" or ".odp" => (Symbol.SlidePlay, "#FF6D3A"),

            ".epub" or ".azw" or ".azw3" or ".mobi" or ".fb2" or ".djvu" or ".prc" => (Symbol.Book, "#FF8A4F"),

            ".cs" or ".py" or ".java" or ".cpp" or ".c" or ".h" or ".js" or ".ts" or ".html" or ".css" or ".php"
                or ".go"
                or ".rs"
                => (Symbol.Code, "#7ED4E6"),

            _ => (Symbol.Document, "#FFFFFF")
        };

        try
        {
            IconSymbol = icon;
            IconColor = new ImmutableSolidColorBrush(Color.Parse(color));
        }
        catch (Exception ex)
        {
            SkEditorAPI.Logs.Error($"Something went wrong loading {icon}: {ex.Message}");
        }
    }

    public override Symbol? IconSymbol { get; }
    public override IImmutableSolidColorBrush IconColor { get; }

    public override Task LoadChildren()
    {
        return Task.CompletedTask;
    }
}