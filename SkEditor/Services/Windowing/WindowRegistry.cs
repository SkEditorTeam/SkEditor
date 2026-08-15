using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class WindowRegistry(IClassicDesktopStyleApplicationLifetime desktop)
{
    private readonly List<MainWindow> _windows = [];
    public MainWindow? Active => _windows.LastOrDefault(w => w.IsActive) ?? _windows.LastOrDefault();

    public IReadOnlyList<MainWindow> Windows => _windows.AsReadOnly();

    public void Register(MainWindow window)
    {
        _windows.Add(window);
        window.Closed += (_, _) => _windows.Remove(window);
    }

    public Window FindTopmostWindow(Window rootWindow)
    {
        Window current = rootWindow;

        while (true)
        {
            Window? child = desktop.Windows.FirstOrDefault(w => w.Owner == current && w.IsVisible);

            if (child == null)
            {
                return current;
            }

            current = child;
        }
    }
}