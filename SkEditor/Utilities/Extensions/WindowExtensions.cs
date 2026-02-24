using System.Threading.Tasks;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using MainWindow = SkEditor.Views.Windows.MainWindow;

namespace SkEditor.Utilities.Extensions;

public static class WindowExtensions
{
    extension(AppWindow window)
    {
        public async Task ShowDialogOnMainWindow()
        {
            MainWindow? mainWindow = SkEditorAPI.Windows.GetMainWindow();
            if (mainWindow == null)
            {
                return;
            }

            await window.ShowDialog(mainWindow);
        }

        public void ShowOnMainWindow()
        {
            MainWindow? mainWindow = SkEditorAPI.Windows.GetMainWindow();
            if (mainWindow == null)
            {
                return;
            }

            window.Show(mainWindow);
        }

        public async Task<T> ShowDialogOnMainWindow<T>()
        {
            MainWindow? mainWindow = SkEditorAPI.Windows.GetMainWindow();
            if (mainWindow == null)
            {
                return default!;
            }

            return await window.ShowDialog<T>(mainWindow);
        }
    }
}