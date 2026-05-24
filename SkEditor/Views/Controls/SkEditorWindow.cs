using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;

namespace SkEditor.Views.Controls;

public class SkEditorWindow : FAAppWindow
{
    protected SkEditorWindow()
    {
        Focusable = true;

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };

        CanMaximize = false;
        CanMinimize = false;
        CanResize = false;
        ShowAsDialog = true;

        Background = Application.Current?.FindResource("SmallWindowBackgroundColor") as IBrush;
    }

    protected override Type StyleKeyOverride => typeof(FAAppWindow);
}