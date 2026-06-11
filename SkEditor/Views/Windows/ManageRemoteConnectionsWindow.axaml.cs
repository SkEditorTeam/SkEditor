using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SkEditor.Services.Localization;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Windows;
using SkEditor.Views.Controls;

namespace SkEditor.Views.Windows;

public partial class ManageRemoteConnectionsWindow : SkEditorWindow
{
    public ManageRemoteConnectionsWindow() => InitializeComponent();

    public ManageRemoteConnectionsWindow(RemoteConnectionViewModel viewModel) : this()
    {
        DataContext = viewModel;
        SetupViewModelEvents(viewModel);
    }

    private void SetupViewModelEvents(RemoteConnectionViewModel vm)
    {
        vm.RequestKeyFilePicker = SelectKeyFile;
        vm.PasteCompleted += () => HostTextBox.CaretIndex = HostTextBox.Text?.Length ?? 0;
    }

    // move this to the view model !!!
    private async Task<string?> SelectKeyFile()
    {
        TopLevel? topLevel = GetTopLevel(this);
        if (topLevel == null)
        {
            return null;
        }

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = TranslationService.Current.Get(TranslationKey.RemoteConnectionSelectKeyFile),
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType(TranslationService.Current.Get(TranslationKey.RemoteConnectionSelectKeyKeyFiles))
                    { Patterns = ["*.pem", "*.key", "*.ppk", "*.*"] },
                new FilePickerFileType(TranslationService.Current.Get(TranslationKey.RemoteConnectionSelectKeyAllFiles))
                    { Patterns = ["*.*"] }
            ]
        });

        return files.Count >= 1
            ? files[0].Path.LocalPath
            : null;
    }
}