using Avalonia;
using SkEditor.Appearance.Icons;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Layout.Sidebar.UI;

public interface ISidebarPanelViewModel
{
    StandardIcon Icon { get; }
    Thickness IconOffset => new(0, 0, 0, 0);
    TranslatableString Name { get; }
}