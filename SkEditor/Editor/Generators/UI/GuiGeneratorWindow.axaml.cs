using System;
using SkEditor.Windowing.UI;

namespace SkEditor.Editor.Generators.UI;

public partial class GuiGeneratorWindow : SkEditorWindow
{
    public GuiGeneratorWindow() => InitializeComponent();

    public GuiGeneratorWindow(GuiGeneratorWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
        
        Closed += (_, _) => viewModel.Dispose();
    }
}