using SkEditor.Windowing.UI;

namespace SkEditor.Editor.UI.Windows;

public partial class GoToLineWindow : SkEditorWindow
{
    public GoToLineWindow() => InitializeComponent();

    public GoToLineWindow(GoToLineWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;

        viewModel.CloseRequested += Close;
    }
}