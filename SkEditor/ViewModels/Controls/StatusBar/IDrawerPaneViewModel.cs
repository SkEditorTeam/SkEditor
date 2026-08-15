using R3;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls;

public interface IDrawerPaneViewModel
{
    string Id { get; }
    BindableReactiveProperty<bool> IsVisible { get; }
    TranslatableString Name { get; }
    IDrawerToolbarViewModel? Toolbar => null;
}