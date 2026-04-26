using System.Collections.Generic;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models;

public class SettingCategoryNode(TranslationKey titleKey, Symbol symbol)
{
    public TranslatableString Title { get; } = new(titleKey);
    public SymbolIconSource Icon { get; } = new() { Symbol = symbol };
    public List<SettingElement> Elements { get; } = [];
    public List<SettingCategoryNode> Subcategories { get; } = [];
}