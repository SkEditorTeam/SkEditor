using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using SkEditor.Views.Windows;

namespace SkEditor.Contracts.Services.Windowing;

public interface IStartupService
{
    string[]? StartupArguments { get; set; }

    Task CompleteApplicationStartAsync(IClassicDesktopStyleApplicationLifetime desktop, SplashScreen? splashScreen);
}