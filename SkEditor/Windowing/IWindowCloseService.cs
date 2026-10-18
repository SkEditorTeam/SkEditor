using System.Threading.Tasks;
using Avalonia.Controls;

namespace SkEditor.Contracts.Services.Windowing;

public interface IWindowCloseService
{
    bool IsClosing { get; }
    Task<bool> OnWindowClosing(double height, double width, double x, double y, WindowState windowState);
}