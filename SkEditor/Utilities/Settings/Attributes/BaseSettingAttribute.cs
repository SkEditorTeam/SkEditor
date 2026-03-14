using System;
using SkEditor.Utilities.Localization;

#pragma warning disable CS9113 // Parameter is unread.

namespace SkEditor.Utilities.Settings.Attributes;

public abstract class BaseSettingAttribute(
    TranslationKey titleKey,
    TranslationKey? categoryKey = null,
    TranslationKey? descriptionKey = null)
    : Attribute;