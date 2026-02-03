using System;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Localization;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.Utilities.Settings.Attributes;

public abstract class BaseSettingAttribute(
    string titleKey,
    Symbol icon,
    string? categoryKey = null,
    string? descriptionKey = null)
    : Attribute
{
    public TranslatableString TitleKey { get; } = new(titleKey);
    public Symbol Icon { get; } = icon;
    public TranslatableString CategoryKey { get; } = new(categoryKey ?? "SettingsGeneralTitle");
    public TranslatableString? DescriptionKey { get; } = descriptionKey == null ? null : new TranslatableString(descriptionKey);
}