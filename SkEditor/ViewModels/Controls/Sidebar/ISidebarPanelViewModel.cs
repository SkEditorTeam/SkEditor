using SkEditor.Models.UI;

namespace SkEditor.ViewModels.Controls;

public interface ISidebarPanelViewModel
{
    string Name { get; }
    StandardIcon Icon { get; }
}