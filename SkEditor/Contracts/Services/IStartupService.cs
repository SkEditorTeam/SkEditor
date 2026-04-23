using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using SkEditor.Views.Windows;

namespace SkEditor.Contracts.Services;

public interface IStartupService
{
    Task CompleteApplicationStartAsync(IClassicDesktopStyleApplicationLifetime desktop, SplashScreen? splashScreen);
}