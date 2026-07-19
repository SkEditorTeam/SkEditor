using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkEditor.Contracts.Services.Localization;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class LanguageChoiceProvider(ITranslationService translationService) : ISettingsChoiceProvider<string>
{
    private static readonly Comparer<string> Comparer = Comparer<string>.Create((x, y) =>
    {
        if (x == "English")
        {
            return -1;
        }

        return y == "English" ? 1 : string.CompareOrdinal(x, y);
    });

    public IEnumerable<string> GetValues()
    {
        List<string> availableLanguages = [];

        string[] files = Directory.GetFiles(translationService.LanguagesFolder);
        availableLanguages.AddRange(files
            .Select(Path.GetFileNameWithoutExtension)
            .Where(x => x is not null)
            .Cast<string>());

        availableLanguages.Add("English");
        availableLanguages.Sort(Comparer);

        return availableLanguages.Distinct();
    }
}