using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Views.Controls;

public partial class MainMenuControl : UserControl
{
    public MainMenuControl()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not MainMenuViewModel)
        {
            DataContext = App.Services.GetRequiredService<MainMenuViewModel>();
            return;
        }

        base.OnDataContextChanged(e);
    }
}