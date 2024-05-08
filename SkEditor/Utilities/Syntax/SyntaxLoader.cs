using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace SkEditor.Utilities.Syntax;
public static class SyntaxLoader
{
    public static void Load(TextEditor editor)
    {
        var _registryOptions = new LocalRegistryOptions();
        var _textMateInstallation = editor.InstallTextMate(_registryOptions);

        _textMateInstallation.SetGrammar("source.sk");
        _textMateInstallation.SetTheme(_registryOptions.GetDefaultTheme());
    }

    class LocalRegistryOptions : IRegistryOptions
    {
        public ICollection<string> GetInjections(string scopeName)
        {
            return null;
        }

        public IRawGrammar GetGrammar(string scopeName)
        {
            string grammarPath = Path.Combine(AppConfig.AppDataFolderPath, "New Syntax Highlighting",
                "skript/syntaxes/skript.tmLanguage.json");

            using StreamReader reader = new(grammarPath);
            return GrammarReader.ReadGrammarSync(reader);
        }

        public IRawTheme GetTheme(string scopeName)
        {
            string themePath = Path.Combine(AppConfig.AppDataFolderPath, "New Syntax Highlighting",
                "skript/themes/dark_skript.json");

            using StreamReader reader = new(themePath);
            return ThemeReader.ReadThemeSync(reader);
        }

        public IRawTheme GetDefaultTheme()
        {
            string themePath = Path.Combine(AppConfig.AppDataFolderPath, "New Syntax Highlighting",
                "skript/themes/dark_skript.json");

            using StreamReader reader = new(themePath);
            return ThemeReader.ReadThemeSync(reader);
        }
    }
}
