using Avalonia.Data.Converters;
using SkEditor.Core.Common;
using SkEditor.Editor.Generators.ViewModels;

namespace SkEditor.Editor.Generators.Converters;

public static class GuiGeneratorViewConverters
{
    public static IValueConverter ViewModeToIndex => new TwoWayValueConverter<GuiGeneratorViewMode, int>(
        mode => (int)mode,
        idx => (GuiGeneratorViewMode)idx
    );

    public static IValueConverter IsPaletteView => new FuncValueConverter<GuiGeneratorViewMode, bool>(v => v == GuiGeneratorViewMode.Palette);
    public static IValueConverter IsPropertiesView => new FuncValueConverter<GuiGeneratorViewMode, bool>(v => v == GuiGeneratorViewMode.Properties);
}