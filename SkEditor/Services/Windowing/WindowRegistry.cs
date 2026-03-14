using System.Collections.Generic;
using System.Linq;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class WindowRegistry
{
    private readonly List<MainWindow> _windows = [];

    public IReadOnlyList<MainWindow> Windows => _windows.AsReadOnly();
    public MainWindow? Active => _windows.LastOrDefault(w => w.IsActive) ?? _windows.LastOrDefault();

    public void Register(MainWindow window)
    {
        _windows.Add(window);
        window.Closed += (_, _) => _windows.Remove(window);
    }
}