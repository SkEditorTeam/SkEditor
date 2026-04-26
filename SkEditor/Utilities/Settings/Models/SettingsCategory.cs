using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Attributes;

namespace SkEditor.Utilities.Settings.Models;

public enum SettingsCategory
{
    [CategoryMetadata(TranslationKey.SettingsGeneralTitle, Symbol.Settings)]
    General,

    [CategoryMetadata(TranslationKey.SettingsUpdatesTitle, Symbol.CloudSync, Parent = General)]
    Updates,

    [CategoryMetadata(TranslationKey.SettingsAdvancedTitle, Symbol.Wrench)]
    Advanced
}