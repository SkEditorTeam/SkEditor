using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Windowing;
using SkEditor.Models.Remote;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Views.Windows;

public partial class RemoteConnectionWindow : AppWindow
{
    public RemoteConnectionWindow()
    {
        InitializeComponent();
        Focusable = true;

        if (DataContext is not RemoteConnectionViewModel vm) return;

        vm.CloseRequested += (_, _) =>
        {
            var info = vm.ConnectionInfo;
            var infoSnapshot = new RemoteConnectionInfo
            {
                Name = info.Name,
                Host = info.Host,
                Port = info.Port,
                Username = info.Username,
                AuthMethod = info.AuthMethod,
                Password = info.Password,
                KeyFilePath = info.KeyFilePath
            };
            Close(infoSnapshot);
        };
        vm.RequestKeyFilePicker = SelectKeyFile;
    }

    private async Task<string?> SelectKeyFile()
    {
        var topLevel = GetTopLevel(this);
        if (topLevel == null) return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Translation.Get(TranslationKey.RemoteConnectionSelectKeyFile),
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType(Translation.Get(TranslationKey.RemoteConnectionSelectKeyKeyFiles)) { Patterns = ["*.pem", "*.key", "*.ppk", "*.*"] },
                new FilePickerFileType(Translation.Get(TranslationKey.RemoteConnectionSelectKeyAllFiles)) { Patterns = ["*.*"] }
            ]
        });

        return files.Count >= 1
            ? files[0].Path.LocalPath
            : null;
    }
}