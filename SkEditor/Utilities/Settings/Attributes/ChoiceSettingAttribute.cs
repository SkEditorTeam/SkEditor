using System;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Attributes;
#pragma warning disable CS9113 // Parameter is unread.

/// <summary>
///     Marks a property as a choice setting with a specific provider.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ChoiceSettingAttribute(
    TranslationKey titleKey,
    Type providerType,
    TranslationKey descriptionKey = TranslationKey.None)
    : BaseSettingAttribute(titleKey, descriptionKey);