using System.Threading.Tasks;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Views;

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
}