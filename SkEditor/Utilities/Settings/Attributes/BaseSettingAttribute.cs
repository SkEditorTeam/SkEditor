using System;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Attributes;

public abstract class BaseSettingAttribute(
    TranslationKey titleKey,
    TranslationKey? categoryKey = null,
    TranslationKey? descriptionKey = null)
    : Attribute
{
    public TranslatableString TitleKey { get; } = new(titleKey);
    public TranslatableString CategoryKey { get; } = new(categoryKey ?? TranslationKey.SettingsGeneralTitle);
    public TranslatableString DescriptionKey { get; } = new(descriptionKey ?? TranslationKey.None);
}