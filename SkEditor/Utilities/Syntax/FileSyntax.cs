using System.IO;
using System.Threading.Tasks;
using System.Xml;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using Newtonsoft.Json;
using Serilog;

namespace SkEditor.Utilities.Syntax;

/// <summary>
///     Represent a file type syntax, such as YAML, JSON, etc...
///     It is split into the syntax file highlighting and the config file, containing extensions and some other stuff.
/// </summary>
public class FileSyntax
{
    public static readonly FileSyntaxConfig DefaultSkriptConfig = new()
    {
        SyntaxName = "Default",
        LanguageName = "Skript",
        Extensions = [".sk", ".skript"],
        Version = "1.0"
    };

    private FileSyntax(IHighlightingDefinition? highlighting, FileSyntaxConfig? config,
        string folderName)
    {
        Highlighting = highlighting;
        Config = config;
        FolderName = folderName;
    }

    public IHighlightingDefinition? Highlighting { get; private set; }

    public FileSyntaxConfig? Config { get; private set; }

    public string FolderName { get; set; }

    public static async Task<FileSyntax> LoadSyntax(string folder)
    {
        try
        {
            string configFile = Path.Combine(folder, "config.json");
            string syntaxFile = Path.Combine(folder, "syntax.xshd");

            if (!File.Exists(configFile) || !File.Exists(syntaxFile))
            {
                return new FileSyntax(null, null, folder);
            }

            FileSyntaxConfig? config =
                JsonConvert.DeserializeObject<FileSyntaxConfig>(await File.ReadAllTextAsync(configFile));

            if (config == null)
            {
                Log.Error("Failed to deserialize syntax config from {ConfigFile}", configFile);
                return new FileSyntax(null, null, folder);
            }

            StreamReader streamReader = new(syntaxFile);
            XmlReader reader = XmlReader.Create(streamReader);
            IHighlightingDefinition? highlightingDefinition =
                HighlightingLoader.Load(reader, HighlightingManager.Instance);
            streamReader.Close();
            reader.Close();

            return new FileSyntax(highlightingDefinition, config, folder);
        }
        catch (IOException e)
        {
            Log.Error(e, "Failed to load syntax from {Folder}", folder);
            return new FileSyntax(null, null, folder);
        }
    }

    public class FileSyntaxConfig
    {
        public string SyntaxName { get; set; } = string.Empty;

        public string LanguageName { get; set; } = string.Empty;

        public string[] Extensions { get; set; } = [];

        public string Version { get; set; } = string.Empty;

        public string FullIdName => $"{LanguageName}-{SyntaxName}";
    }
}