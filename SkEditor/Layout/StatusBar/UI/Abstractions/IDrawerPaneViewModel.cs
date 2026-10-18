using R3;
using SkEditor.Core.Localization;

namespace SkEditor.Layout.StatusBar.UI.Abstractions;

public interface IDrawerPaneViewModel
{
    string Id { get; }
    BindableReactiveProperty<bool> IsVisible { get; }
    TranslatableString Name { get; }
    IDrawerToolbarViewModel? Toolbar => null;
}