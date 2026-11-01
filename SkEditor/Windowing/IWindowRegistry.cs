using Avalonia.Controls;
using SkEditor.Windowing.UI;

namespace SkEditor.Windowing;

public interface IWindowRegistry
{
    MainWindow? Active { get; }
    void Register(MainWindow window);
    Window FindTopmostWindow(Window rootWindow);
}