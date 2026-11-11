using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Editor.Generators.ViewModels;

public partial class GuiItemViewModel(string id, string name, Bitmap? image) : ObservableObject
{
    public string Id { get; } = id;
    public Bitmap? Image { get; } = image;

    [ObservableProperty]
    public partial string Name { get; set; } = name;
    
    [ObservableProperty]
    public partial string? DisplayName { get; set; }

    [ObservableProperty]
    public partial int Amount { get; set; } = 1;
    
    [ObservableProperty]
    public partial bool GenerateExampleAction { get; set; } = false;
    
    public bool IsBackground { get; set; }
}