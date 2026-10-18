using Avalonia;
using SkEditor.Models.UI;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls;

public interface ISidebarPanelViewModel
{
    StandardIcon Icon { get; }
    Thickness IconOffset => new(0, 0, 0, 0);
    TranslatableString Name { get; }
}