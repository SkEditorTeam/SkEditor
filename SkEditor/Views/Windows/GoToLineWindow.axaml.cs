using Avalonia.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Views.Windows;

public partial class GoToLineWindow : FAAppWindow
{
    public GoToLineWindow()
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

    public GoToLineWindow(GoToLineWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;

        viewModel.CloseRequested += Close;
    }
}