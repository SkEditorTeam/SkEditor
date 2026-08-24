using System;
using System.Collections.Generic;
using System.IO;
using SkEditor.Models.UI;

namespace SkEditor.Contracts.Services;

public interface ISyntaxService
{
    string SyntaxFolderPath { get; }
    string GrammarFolder { get; }
    string ThemeFolder { get; }
    void LoadSyntaxes();

    string? GetGrammarFileByScope(string scopeName);
    SyntaxPair? GetPairByExtension(string? extension);

    Stream? GetGrammarStream(string fileName);
    Stream? GetThemeStream(string fileName);

    List<SyntaxTheme> AvailableThemes { get; }
    List<SyntaxGrammar> AvailableGrammars { get; }

    event Action SyntaxesReloaded;
}