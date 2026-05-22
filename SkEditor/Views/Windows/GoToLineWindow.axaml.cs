using SkEditor.ViewModels.Windows;
using SkEditor.Views.Controls;

namespace SkEditor.Views.Windows;

public partial class GoToLineWindow : SkEditorWindow
{
    public GoToLineWindow() => InitializeComponent();

    public GoToLineWindow(GoToLineWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;

        viewModel.CloseRequested += Close;
    }
}