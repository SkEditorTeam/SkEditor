using Avalonia.Data.Converters;
using SkEditor.Core.Common;
using SkEditor.Editor.Generators.ViewModels;

namespace SkEditor.Editor.Generators.Converters;

public static class GuiGeneratorViewConverters
{
    public static IValueConverter ViewModeToIndex => new TwoWayValueConverter<GuiGeneratorTab, int>(
        mode => (int)mode,
        idx => (GuiGeneratorTab)idx
    );

    public static IValueConverter IsPaletteView => new FuncValueConverter<GuiGeneratorTab, bool>(v => v == GuiGeneratorTab.Palette);
    public static IValueConverter IsPropertiesView => new FuncValueConverter<GuiGeneratorTab, bool>(v => v == GuiGeneratorTab.Properties);
}