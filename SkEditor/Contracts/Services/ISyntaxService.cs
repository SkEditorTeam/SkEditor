using System.IO;
using SkEditor.Models.UI;

namespace SkEditor.Contracts.Services;

public interface ISyntaxService
{
    string SyntaxFolderPath { get; set; }
    void LoadSyntaxes();
    SyntaxPair? GetPairByExtension(string? extension);

    Stream GetThemeStream(string? extension);
}