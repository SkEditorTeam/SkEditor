using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.ViewModels;

namespace SkEditor.Views.Windows.FileTypes;

public partial class FileTypeSelectionWindow : AppWindow
{
    public FileTypeSelectionWindow()
    {
        InitializeComponent();

        Focusable = true;
        AssignCommands();
    }

    public void AssignCommands()
    {
        CancelButton.Command = new RelayCommand(() =>
        {
            FileTypeSelectionViewModel? viewModel = (FileTypeSelectionViewModel?)DataContext;
            if (viewModel == null)
                return;
            viewModel.SelectedFileType = null;
            Close();
        });
        OpenButton.Command = new RelayCommand(Close);

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
    }
}