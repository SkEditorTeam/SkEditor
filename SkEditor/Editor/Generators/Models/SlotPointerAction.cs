using SkEditor.Editor.Generators.ViewModels;

namespace SkEditor.Editor.Generators.Models;

public enum ClickType
{
    Left,
    Middle,
    Right
}
public record SlotPointerAction(SlotViewModel Slot, ClickType ClickType);