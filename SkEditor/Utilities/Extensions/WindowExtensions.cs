using System.Threading.Tasks;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Views;
using MainWindow = SkEditor.Views.Windows.MainWindow;

namespace SkEditor.Utilities.Extensions;

public static class WindowExtensions
{
    public static async Task ShowDialogOnMainWindow(this AppWindow window)
    {
        MainWindow? mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow == null) return;
        await window.ShowDialog(mainWindow);
    }
    
    public static void ShowOnMainWindow(this AppWindow window)
    {
        MainWindow? mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow == null) return;
        window.Show(mainWindow);
    }
    
    public static async Task<T> ShowDialogOnMainWindow<T>(this AppWindow window)
    {
        MainWindow? mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow == null) return default!;
        return await window.ShowDialog<T>(mainWindow);
    }
}