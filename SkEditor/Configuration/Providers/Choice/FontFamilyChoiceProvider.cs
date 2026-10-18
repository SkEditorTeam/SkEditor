using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;

namespace SkEditor.Configuration.Providers.Choice;

public class FontFamilyChoiceProvider : ISettingsChoiceProvider<string?>
{
    public IEnumerable<string?> GetValues()
    {
        string[] systemFonts =
        [
            .. FontManager.Current.SystemFonts
                .Select(family => family.Name)
        ];

        List<string?> fonts = new(systemFonts.Length + 1) { null };
        fonts.AddRange(systemFonts);

        return fonts;
    }

    public TranslatableString GetDisplayValue(string? value)
    {
        if (value is null)
        {
            return TranslationKey.SettingsAppearanceCurrentFontDescriptionDefault;
        }

        return value.ToTranslatable();
    }
}