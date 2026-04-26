using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkEditor.Contracts.Services.Localization;

namespace SkEditor.Utilities.Settings.Providers;

public class LanguageSettingsProvider(ITranslationService translationService) : ISettingsProvider
{
    public IEnumerable<object> GetValues()
    {
        List<string> availableLanguages = [];

        string[] files = Directory.GetFiles(translationService.LanguagesFolder);
        availableLanguages.AddRange(files
            .Select(Path.GetFileNameWithoutExtension)
            .Where(x => x is not null)
            .Cast<string>());

        availableLanguages.Add("English");
        availableLanguages.Sort();

        return availableLanguages.Distinct();
    }
}