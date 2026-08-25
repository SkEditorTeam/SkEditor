using System;
using System.Collections.Generic;
using System.IO;
using SkEditor.Models.UI;
using TextMateSharp.Internal.Types;
using TextMateSharp.Themes;

namespace SkEditor.Contracts.Services;

public interface ISyntaxService
{
    string SyntaxFolderPath { get; }
    string GrammarFolder { get; }
    string ThemeFolder { get; }
    void LoadSyntaxes();
    void RefreshPairs();

    string? GetGrammarFileByScope(string scopeName);
    SyntaxPair? GetPairByExtension(string? extension);

    IRawGrammar? GetRawGrammar(string scopeName);
    IRawTheme? GetRawTheme(string themeId);
    IRawTheme GetDefaultTheme();
    
    Stream? GetGrammarStream(string fileName);
    Stream? GetThemeStream(string fileName);

    List<SyntaxTheme> AvailableThemes { get; }
    List<SyntaxGrammar> AvailableGrammars { get; }

    event Action SyntaxesReloaded;
}