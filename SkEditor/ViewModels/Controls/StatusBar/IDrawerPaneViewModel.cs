using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls;

public interface IDrawerPaneViewModel
{
    TranslatableString Name { get; }
    IDrawerToolbarViewModel? Toolbar => null;
}