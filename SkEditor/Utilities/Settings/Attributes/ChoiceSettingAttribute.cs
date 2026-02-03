using System;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Providers;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.Utilities.Settings.Attributes;

/// <summary>
///     Marks a property as a choice setting with a specific provider.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ChoiceSettingAttribute(
    string titleKey, 
    Symbol icon, 
    string categoryId, Type providerType, string? descriptionKey = null)
    : BaseSettingAttribute(titleKey, icon, categoryId, descriptionKey);