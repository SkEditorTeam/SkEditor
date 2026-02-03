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
    string categoryId = "General",
    string? descriptionKey = null)
    : BaseSettingAttribute(titleKey, categoryId, descriptionKey);