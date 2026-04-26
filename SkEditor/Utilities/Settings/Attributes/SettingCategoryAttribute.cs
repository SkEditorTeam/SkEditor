using System;
using SkEditor.Utilities.Settings.Models;

namespace SkEditor.Utilities.Settings.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SettingCategoryAttribute(SettingsCategory category) : Attribute;