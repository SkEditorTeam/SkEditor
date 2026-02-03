using System;

namespace SkEditor.Utilities.Settings.Attributes;

/// <summary>
///     Marks a property as a choice setting with a specific provider.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ChoiceSettingAttribute(
    string titleKey,
    Type providerType,
    string? categoryId = null,
    string? descriptionKey = null)
    : BaseSettingAttribute(titleKey, categoryId, descriptionKey);