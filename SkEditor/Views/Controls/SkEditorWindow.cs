using System;
using Avalonia.Input;
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
    }

    protected override Type StyleKeyOverride => typeof(FAAppWindow);
}