using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Windowing;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Views.Windows;

public partial class RemoteConnectionWindow : AppWindow
{
    public RemoteConnectionWindow()
    {
        InitializeComponent();
        Focusable = true;

        if (DataContext is not RemoteConnectionViewModel vm) return;

        vm.CloseRequested += (_, _) => Close();
        vm.RequestKeyFilePicker = SelectKeyFile;
    }

    private async Task<string?> SelectKeyFile()
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null) return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Private Key",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Key Files") { Patterns = ["*.pem", "*.key", "*.ppk", "*.*"] },
                new FilePickerFileType("All Files") { Patterns = ["*.*"] }
            ]
        });

        return files.Count >= 1
            ? files[0].Path.LocalPath
            : null;
    }
}