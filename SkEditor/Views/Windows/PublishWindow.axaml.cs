using Avalonia.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Views.Windows;

public partial class PublishWindow : FAAppWindow
{
    public PublishWindow()
    {
        InitializeComponent();
        Focusable = true;
        InitializeUi();
    }

    public PublishWindow(PublishWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void InitializeUi()
    {
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
    }
}