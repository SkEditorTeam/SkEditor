using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkEditor.Services.Localization;

namespace SkEditor.Utilities.Settings.Providers;

public class LanguageSettingsProvider : ISettingsProvider
{
    public IEnumerable<object> GetValues()
    {
        List<string> availableLanguages = [];

        string[] files = Directory.GetFiles(TranslationService.Current.LanguagesFolder);
        availableLanguages.AddRange(files
            .Select(Path.GetFileNameWithoutExtension)
            .Where(x => x is not null)
            .Cast<string>());

        availableLanguages.Add("English");
        availableLanguages.Sort();

        return availableLanguages.Distinct();
    }
}