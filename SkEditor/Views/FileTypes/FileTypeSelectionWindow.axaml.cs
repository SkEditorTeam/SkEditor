using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.ViewModels;

namespace SkEditor.Views.FileTypes;

public partial class FileTypeSelectionWindow : AppWindow
{
    public FileTypeSelectionWindow()
    {
        InitializeComponent();

        AssignCommands();
    }

    public void AssignCommands()
    {
        CancelButton.Command = new RelayCommand(() =>
        {
            ((FileTypeSelectionViewModel)DataContext).SelectedFileType = null;
            Close();
        });
        OpenButton.Command = new RelayCommand(Close);
    }
}