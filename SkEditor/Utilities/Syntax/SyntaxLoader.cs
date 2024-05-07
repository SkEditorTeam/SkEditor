using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextMateSharp.Grammars;
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
            return null;
        }

        public IRawTheme GetTheme(string scopeName)
        {
            return null;
        }

        public IRawTheme GetDefaultTheme()
        {
            string themePath = Path.GetFullPath(
                @"C:\Users\Notro\Desktop\Projekty\SkEditor\SkEditor\Assets\dark_sk.json");

            using StreamReader reader = new(themePath);
            return ThemeReader.ReadThemeSync(reader);
        }
    }
}
