using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Utilities.Localization;

namespace SkEditor.Services.Files;

public class FilePickerService(ITranslationService translations) : IFilePickerService
{
    public async Task<string[]?> PickFilesAsync(bool allowMultiple = true)
    {
        IStorageProvider? storageProvider = GetStorageProvider();
        if (storageProvider == null)
        {
            return null;
        }

        FilePickerOpenOptions dialogOptions = new()
        {
            Title = translations.Get(TranslationKey.WindowTitleOpenFilePicker),
            AllowMultiple = allowMultiple
        };
        IReadOnlyList<IStorageFile> result = await storageProvider.OpenFilePickerAsync(dialogOptions);
        return result.Count == 0 ? null : result.Select(x => x.Path.LocalPath).ToArray();
    }

    public async Task<string[]?> PickFoldersAsync(bool allowMultiple = true)
    {
        IStorageProvider? storageProvider = GetStorageProvider();
        if (storageProvider == null)
        {
            return null;
        }

        FolderPickerOpenOptions dialogOptions = new()
        {
            Title = translations.Get(TranslationKey.WindowTitleOpenFolderPicker),
            AllowMultiple = allowMultiple
        };

        IReadOnlyList<IStorageFolder> result = await storageProvider.OpenFolderPickerAsync(dialogOptions);
        return result.Count == 0 ? null : result.Select(x => x.Path.LocalPath).ToArray();
    }

    public async Task<string?> SaveFileAsync(string fileName)
    {
        IStorageProvider? storageProvider = GetStorageProvider();
        if (storageProvider == null)
        {
            return null;
        }

        FilePickerSaveOptions dialogOptions = new()
        {
            Title = translations.Get(TranslationKey.WindowTitleSaveFilePicker, fileName)
        };
        IStorageFile? result = await storageProvider.SaveFilePickerAsync(dialogOptions);
        return result?.Path.LocalPath;
    }

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return TopLevel.GetTopLevel(desktop.MainWindow)?.StorageProvider;
        return null;
    }
}