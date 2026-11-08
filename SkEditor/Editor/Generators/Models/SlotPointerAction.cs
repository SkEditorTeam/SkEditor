using Avalonia.Input;
using SkEditor.Editor.Generators.ViewModels;

namespace SkEditor.Editor.Generators.Models;

public record SlotPointerAction(SlotViewModel Slot, PointerUpdateKind PointerKind);