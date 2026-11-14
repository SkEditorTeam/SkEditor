using SkEditor.Editor.Generators.ViewModels;
using SkEditor.Windowing.UI;

namespace SkEditor.Editor.Generators.UI;

public partial class GuiGeneratorWindow : SkEditorWindow
{
    public GuiGeneratorWindow() => InitializeComponent();

    public GuiGeneratorWindow(GuiGeneratorWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
        
        Loaded += async (_, _) =>
        {
            await viewModel.ItemPalette.LoadCommand.ExecuteAsync(null);
        };
    }
}