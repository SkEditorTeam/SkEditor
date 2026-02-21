using System;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Attributes;

/// <summary>
///     Marks a property as a choice setting with a specific provider.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ChoiceSettingAttribute(
    TranslationKey titleKey,
    Type providerType,
    TranslationKey categoryKey = TranslationKey.None,
    TranslationKey descriptionKey = TranslationKey.None)
    : BaseSettingAttribute(titleKey, categoryKey, descriptionKey)
{
    public Type ProviderType { get; } = providerType;
}