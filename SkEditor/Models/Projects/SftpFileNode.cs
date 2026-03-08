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

            ".java" => (Symbol.DocumentJava, "#E76F00"),
            ".css" or ".scss" or ".sass" => (Symbol.DocumentCss, "#1572B6"),
            ".sql" => (Symbol.Database, "#00758F"),

            ".yml" or ".yaml" => (Symbol.DocumentYml, "#D32F2F"),
            ".json" or ".json5" => (Symbol.Braces, "#FFB300"),
            ".toml" or ".properties" or ".ini" or ".cfg" or ".conf" => (Symbol.DocumentSettings,
                "#78909C"),

            ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".webp" or ".svg"
                or ".ico" => (Symbol.Image, "#29B6F6"),
            ".mp3" or ".wav" or ".ogg" or ".flac" => (Symbol.SoundWaveCircle, "#BA68C8"),
            ".mp4" or ".mkv" or ".avi" or ".mov" => (Symbol.Video, "#FF5252"),

            ".txt" or ".info" => (Symbol.DocumentText, "#CFD8DC"),
            ".log" => (Symbol.DocumentHeader, "#90A4AE"),
            ".md" => (Symbol.DocumentBulletList, "#4FC3F7"),

            ".dll" or ".so" or ".bin" or ".sys" => (Symbol.DocumentCube, "#955251"),
            ".exe" or ".msi" => (Symbol.AppGeneric, "#B71C1C"),
            ".jar" => (Symbol.Box, "#DD4124"),

            ".bat" or ".cmd" => (Symbol.WindowConsole, "#4CAF50"),
            ".sh" or ".bash" or ".zsh" => (Symbol.WindowConsole, "#263238"),
            ".ps1" => (Symbol.WindowConsole, "#0078D4"),

            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" or ".xz" => (Symbol.FolderZip,
                "#FFA726"),

            ".env" or ".pem" or ".key" or ".pub" or ".cert" => (Symbol.DocumentKey,
                "#FFD600"),

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