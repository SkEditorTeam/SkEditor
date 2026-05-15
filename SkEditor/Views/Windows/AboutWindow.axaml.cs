using Avalonia.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Views.Windows;

public partial class AboutWindow : FAAppWindow
{
    public AboutWindow()
    {
        InitializeComponent();

        KeyDown += (_, e) =>
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
            }
        };
    }

    public AboutWindow(AboutWindowViewModel viewModel) : this() => DataContext = viewModel;
}