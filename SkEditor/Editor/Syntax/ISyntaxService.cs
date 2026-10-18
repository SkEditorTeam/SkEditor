using System;
using System.Collections.Generic;
using System.IO;
using SkEditor.Editor.Syntax.Models;
using TextMateSharp.Internal.Types;
using TextMateSharp.Themes;

namespace SkEditor.Editor.Syntax;

/// <summary>
///     Manages the syntax highlighting, including loading grammars, themes, and their associations.
/// </summary>
public interface ISyntaxService
{
    /// <summary>
    ///     Gets the path to the folder where syntax files (grammars and themes) are stored.
    /// </summary>
    string SyntaxFolderPath { get; }

    /// <summary>
    ///     Gets the path to the folder where grammar files are stored.
    /// </summary>
    string GrammarFolder { get; }

    /// <summary>
    ///     Gets the path to the folder where theme files are stored.
    /// </summary>
    string ThemeFolder { get; }

    /// <summary>
    ///     Loads all syntax files (grammars and themes) from their folders.
    /// </summary>
    void LoadSyntaxes();

    /// <summary>
    ///     Refreshes the associations between file extensions and syntax pairs after they were changed in the settings.
    /// </summary>
    void RefreshPairs();

    /// <summary>
    ///     Gets the grammar file name associated with the given scope name.
    /// </summary>
    string? GetGrammarFileByScope(string scopeName);

    /// <summary>
    ///     Gets the syntax pair associated with the given file extension.
    /// </summary>
    SyntaxPair? GetPairByExtension(string? extension);

    /// <summary>
    ///     Gets the raw grammar associated with the given scope name.
    /// </summary>
    IRawGrammar? GetRawGrammar(string scopeName);

    /// <summary>
    ///     Gets the raw theme associated with the given theme ID.
    /// </summary>
    IRawTheme? GetRawTheme(string themeId);

    /// <summary>
    ///     Gets the registry default raw theme.
    /// </summary>
    IRawTheme GetDefaultTheme();

    /// <summary>
    ///     Retrieves a stream for the specified grammar
    /// </summary>
    Stream? GetGrammarStream(string fileName);

    /// <summary>
    ///     Retrieves a stream for the specified theme
    /// </summary>
    Stream? GetThemeStream(string fileName);

    /// <summary>
    ///     Gets the list of available syntax themes
    /// </summary>
    List<SyntaxTheme> AvailableThemes { get; }

    /// <summary>
    ///     Gets the list of available syntax grammars
    /// </summary>
    List<SyntaxGrammar> AvailableGrammars { get; }

    /// <summary>
    ///     Fires when the syntaxes (grammars or themes) have been reloaded
    /// </summary>
    event Action SyntaxesReloaded;
}