using System.IO;
using System.Xml;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using Newtonsoft.Json;
using SkEditor.Views.Settings.Personalization;

namespace SkEditor.Utilities.Syntax;

/// <summary>
/// Represent a file type syntax, such as YAML, JSON, etc...
/// It is split into the syntax file highlighting and the config file, containing extensions and some other stuff.
/// </summary>
public class FileSyntax
{
    public static FileSyntax LoadSyntax(string folder)
    {
        var configFile = Path.Combine(folder, "config.json");
        var syntaxFile = Path.Combine(folder, "syntax.xshd");

        if (!File.Exists(configFile) || !File.Exists(syntaxFile))
        {
            throw new FileNotFoundException("The syntax folder must contain a config.json and a syntax.xshd file.");
        }
        
        var config = JsonConvert.DeserializeObject<FileSyntaxConfig>(File.ReadAllText(configFile));
        
        StreamReader streamReader = new (syntaxFile);
        var reader = XmlReader.Create(streamReader);
        var highlightingDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        streamReader.Close();
        reader.Close();
        
        return new FileSyntax(highlightingDefinition, config);
    }

    public static FileSyntax LoadAsSkript(string syntaxFile)
    {
        var fileName = Path.GetFileNameWithoutExtension(syntaxFile);
        var config = new FileSyntaxConfig
        {
            SyntaxName = fileName,
            LanguageName = "Skript",
            Extensions = new[] {"sk", "skript"},
            Version = "1.0.0"
        };
        
        StreamReader streamReader = new (syntaxFile);
        var reader = XmlReader.Create(streamReader);
        var highlightingDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        streamReader.Close();
        reader.Close();
        
        return new FileSyntax(highlightingDefinition, config);
    }

    private FileSyntax(IHighlightingDefinition highlighting, FileSyntaxConfig config)
    {
        Highlighting = highlighting;
        Config = config;
    }
    
    public IHighlightingDefinition Highlighting { get; private set; }
    
    public FileSyntaxConfig Config { get; private set; }
    
    public class FileSyntaxConfig
    {
        public string SyntaxName { get; set; }
        
        public string LanguageName { get; set; }
        
        public string[] Extensions { get; set; }
        
        public string Version { get; set; }
    }
}