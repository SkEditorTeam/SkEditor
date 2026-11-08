using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Editor.Generators.ViewModels;

public partial class SlotViewModel(int index) : ObservableObject
{
    public int Index { get; } = index;

    [ObservableProperty]
    public partial GuiItemViewModel? Item { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}