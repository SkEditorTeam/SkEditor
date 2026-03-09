using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FluentIcons.Common;
using Renci.SshNet;

namespace SkEditor.Models.Projects;

public class SftpFileNode : FileSystemNode
{
    public SftpFileNode(string name, string path, SftpClient client) : base(name, path, false)
    {
        string extension = System.IO.Path.GetExtension(name).ToLower();
        (Symbol icon, string color) = extension switch
        {
            ".sk" => (Symbol.Script, "#00C853"),
            ".sql" or ".db" or ".sqlite" or ".sqlite3" => (Symbol.Database, "#BEF26F"),
            ".csv" or ".tsv" => (Symbol.DocumentTable, "#FF8A30"),

            ".yml" or ".yaml" => (Symbol.DocumentYml, "#49B7E3"),
            ".json" or ".json5" => (Symbol.Braces, "#FFB300"),
            ".toml" or ".properties" or ".ini" or ".cfg" or ".conf" => (Symbol.DocumentSettings,
                "#49B7E3"),

            ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".webp" or ".svg"
                or ".ico" => (Symbol.Image, "#29B6F6"),
            ".mp3" or ".wav" or ".ogg" or ".flac" => (Symbol.MusicNote2, "#BCF02E"),
            ".mp4" or ".mkv" or ".avi" or ".mov" => (Symbol.Video, "#FF5252"),

            ".txt" or ".info" => (Symbol.DocumentText, "#CFD8DC"),
            ".log" => (Symbol.DocumentHeader, "#BDCFD9"),
            ".md" => (Symbol.DocumentBulletList, "#4FC3F7"),

            ".dll" or ".so" or ".bin" or ".sys" => (Symbol.DocumentCube, "#F53D3D"),
            ".exe" or ".msi" => (Symbol.Play, "#35D4B4"),
            ".jar" => (Symbol.Box, "#F03765"),

            ".bat" or ".cmd" or ".sh" or ".bash" or ".zsh" or ".ps1" => (Symbol.WindowConsole, "#4AB9ED"),

            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" or ".xz" => (Symbol.FolderZip,
                "#FF87B7"),

            ".env" or ".pem" or ".key" or ".pub" or ".cert" => (Symbol.Key,
                "#E38B2D"),

            _ => (Symbol.Document, "#FFFFFF")
        };

        Icon = icon;
        IconColor = new ImmutableSolidColorBrush(Color.Parse(color));
    }

    public override Symbol? Icon { get; }
    public override IImmutableSolidColorBrush IconColor { get; }

    public override Task LoadChildren()
    {
        return Task.CompletedTask;
    }
}