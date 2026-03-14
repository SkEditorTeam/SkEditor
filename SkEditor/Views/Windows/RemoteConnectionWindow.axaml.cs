using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
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
    }

    public RemoteConnectionWindow(RemoteConnectionViewModel viewModel) : this()
    {
        DataContext = viewModel;
        SetupViewModelEvents(viewModel);
    }

    private void SetupViewModelEvents(RemoteConnectionViewModel vm)
    {
        vm.CloseRequested += (_, _) =>
        {
            RemoteConnectionInfo info = vm.ConnectionInfo;
            RemoteConnectionInfo infoSnapshot = new()
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
        TopLevel? topLevel = GetTopLevel(this);
        if (topLevel == null)
        {
            return null;
        }

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Translation.Get(TranslationKey.RemoteConnectionSelectKeyFile),
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType(Translation.Get(TranslationKey.RemoteConnectionSelectKeyKeyFiles))
                    { Patterns = ["*.pem", "*.key", "*.ppk", "*.*"] },
                new FilePickerFileType(Translation.Get(TranslationKey.RemoteConnectionSelectKeyAllFiles))
                    { Patterns = ["*.*"] }
            ]
        });

        return files.Count >= 1
            ? files[0].Path.LocalPath
            : null;
    }
}