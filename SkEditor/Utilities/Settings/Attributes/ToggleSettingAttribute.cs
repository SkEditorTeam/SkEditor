using System;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Localization;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.Utilities.Settings.Attributes;

/// <summary>
///     Marks a property as a toggleable setting in the settings UI.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ToggleSettingAttribute(
    string titleKey,
    Symbol icon,
    string? categoryKey = null,
    string? descriptionKey = null)
    : Attribute
{
    public TranslatableString TitleKey { get; } = titleKey;
    public Symbol Icon { get; } = icon;
    public TranslatableString CategoryKey { get; } = categoryKey ?? "SettingsGeneralTitle";
    public TranslatableString? DescriptionKey { get; } = descriptionKey;
}