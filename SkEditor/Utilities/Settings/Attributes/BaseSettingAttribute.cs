using System;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Attributes;

public abstract class BaseSettingAttribute(
    string titleKey,
    string? categoryKey = null,
    string? descriptionKey = null)
    : Attribute
{
    public TranslatableString TitleKey { get; } = new(titleKey);
    public TranslatableString CategoryKey { get; } = new(categoryKey ?? "SettingsGeneralTitle");
    public TranslatableString? DescriptionKey { get; } = descriptionKey == null ? null : new TranslatableString(descriptionKey);
}