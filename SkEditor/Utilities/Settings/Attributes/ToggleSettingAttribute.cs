using System;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Attributes;

/// <summary>
///     Marks a property as a toggleable setting in the settings UI.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ToggleSettingAttribute(
    TranslationKey titleKey,
    TranslationKey categoryKey = TranslationKey.SettingsGeneralTitle,
    TranslationKey descriptionKey = TranslationKey.None)
    : BaseSettingAttribute(titleKey, categoryKey, descriptionKey);