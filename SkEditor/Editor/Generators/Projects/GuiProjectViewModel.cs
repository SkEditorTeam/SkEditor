using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Editor.Generators.Projects;

public partial class GuiProjectViewModel(GuiProject model, bool isActive) : ObservableObject
{
    public GuiProject Model { get; } = model;

    [ObservableProperty]
    public partial string Name { get; set; } = model.Name;

    [ObservableProperty]
    public partial bool IsActive { get; set; } = isActive;
}