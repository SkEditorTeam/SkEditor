using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Editor.Generators.Models;

namespace SkEditor.Editor.Generators.ViewModels;

public partial class GuiProjectItemViewModel(GuiProject model, bool isActive) : ObservableObject
{
    public GuiProject Model { get; } = model;

    [ObservableProperty]
    public partial string Name { get; set; } = model.Name;

    [ObservableProperty]
    public partial bool IsActive { get; set; } = isActive;
}