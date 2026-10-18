using R3;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Layout.StatusBar.UI.Abstractions;

public interface IDrawerPaneViewModel
{
    string Id { get; }
    BindableReactiveProperty<bool> IsVisible { get; }
    TranslatableString Name { get; }
    IDrawerToolbarViewModel? Toolbar => null;
}