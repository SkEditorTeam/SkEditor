using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using Newtonsoft.Json;
using Serilog;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace SkEditor.Utilities.Syntax;

/// <summary>
/// Represent a file type syntax, such as YAML, JSON, etc...
/// It is split into the syntax file highlighting and the config file, containing extensions and some other stuff.
/// </summary>
public class FileSyntax
{
    public static FileSyntaxConfig DefaultSkriptConfig = new()
    {
        SyntaxName = "Default",
        LanguageName = "Skript",
        Extensions = [".sk", ".skript"],
        Version = "1.0"
    };

    public static async Task<FileSyntax> LoadSyntax(string folder)
    {
        try
        {
            var configFile = Path.Combine(folder, "config.json");
            var syntaxFile = Path.Combine(folder, "syntax.xshd");

            if (!File.Exists(configFile) || !File.Exists(syntaxFile)) return new FileSyntax(null, null, folder);

            var config = JsonConvert.DeserializeObject<FileSyntaxConfig>(await File.ReadAllTextAsync(configFile));

            StreamReader streamReader = new(syntaxFile);
            var reader = XmlReader.Create(streamReader);
            var highlightingDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
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

    private FileSyntax(IHighlightingDefinition highlighting, FileSyntaxConfig config,
        string folderName)
    {
        Highlighting = highlighting;
        Config = config;
        FolderName = folderName;
    }

    public IHighlightingDefinition Highlighting { get; private set; }

    public FileSyntaxConfig Config { get; private set; }

    public string FolderName { get; set; }

    public class FileSyntaxConfig
    {
        public string SyntaxName { get; set; }

        public string LanguageName { get; set; }

        public string[] Extensions { get; set; }

        public string Version { get; set; }

        public string FullIdName => $"{LanguageName}-{SyntaxName}";
    }
}