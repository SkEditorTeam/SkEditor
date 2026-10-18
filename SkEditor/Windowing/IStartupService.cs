using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using SkEditor.Windowing.UI;

namespace SkEditor.Windowing;

public interface IStartupService
{
    string[]? StartupArguments { get; set; }

    Task CompleteApplicationStartAsync(IClassicDesktopStyleApplicationLifetime desktop, SplashScreen? splashScreen);
}