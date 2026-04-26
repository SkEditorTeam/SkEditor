using System;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Models;

namespace SkEditor.Utilities.Settings.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class CategoryMetadataAttribute(TranslationKey titleKey, Symbol icon) : Attribute
{
    public SettingsCategory Parent { get; set; }
}