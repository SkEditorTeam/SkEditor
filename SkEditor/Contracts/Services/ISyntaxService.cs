using System.IO;
using SkEditor.Models.UI;

namespace SkEditor.Contracts.Services;

public interface ISyntaxService
{
    string SyntaxFolderPath { get; set; }
    void LoadSyntaxes();

    string? GetGrammarFileByScope(string scopeName);
    SyntaxPair? GetPairByExtension(string? extension);

    Stream? GetGrammarStream(string fileName);
    Stream? GetThemeStream(string fileName);
}