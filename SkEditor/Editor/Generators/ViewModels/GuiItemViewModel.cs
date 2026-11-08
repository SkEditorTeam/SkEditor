using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Editor.Generators.ViewModels;

public partial class GuiItemViewModel(string id, string displayName, Bitmap? image) : ObservableObject
{
    public string Id { get; } = id;
    public Bitmap? Image { get; } = image;

    [ObservableProperty]
    public partial string DisplayName { get; set; } = displayName;

    [ObservableProperty]
    public partial int Amount { get; set; } = 1;
    
    public bool IsBackground { get; set; }
}