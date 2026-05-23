using SkEditor.Models.UI;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls;

public interface ISidebarPanelViewModel
{
    TranslatableString Name { get; }
    StandardIcon Icon { get; }
}