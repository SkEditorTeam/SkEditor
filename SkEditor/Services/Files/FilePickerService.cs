using System;
using System.Collections.Generic;
using System.IO;
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
    private readonly FilePickerFileType _skriptFileType = new("Skript")
    {
        Patterns =
        [
            "*.sk"
        ]
    };
    
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
        return result.Count == 0
            ? null
            : result.Select(x =>
            {
                string localPath = x.Path.IsAbsoluteUri
                    ? x.Path.LocalPath
                    : Uri.UnescapeDataString(x.Path.OriginalString);
                return localPath;
            }).ToArray();
    }

    public async Task<string?> SaveFileAsync(string fileName, string? currentPath = null)
    {
        IStorageProvider? storageProvider = GetStorageProvider();
        if (storageProvider == null)
        {
            return null;
        }

        FilePickerSaveOptions dialogOptions = await BuildSaveFilePickerOptions(storageProvider, fileName, currentPath);
        IStorageFile? result = await storageProvider.SaveFilePickerAsync(dialogOptions);
        return result?.Path.LocalPath;
    }

    private async Task<FilePickerSaveOptions> BuildSaveFilePickerOptions(IStorageProvider storageProvider,
        string fileName, string? currentPath)
    {
        string? extension = Path.GetExtension(currentPath)?.ToLowerInvariant();
        bool isSkript = string.IsNullOrEmpty(extension) || extension == ".sk";

        string? folder = null;
        if (currentPath is not null)
        {
            folder = Path.GetDirectoryName(currentPath);
        }

        IStorageFolder? storageFolder = folder is null
            ? null
            : await storageProvider.TryGetFolderFromPathAsync(folder);

        FilePickerSaveOptions dialogOptions = new()
        {
            Title = translations.Get(TranslationKey.WindowTitleSaveFilePicker, fileName),
            SuggestedStartLocation = storageFolder,
            SuggestedFileName = fileName,
            SuggestedFileType = isSkript ? _skriptFileType : null,
            FileTypeChoices =
            [
                new FilePickerFileType(translations.Get(TranslationKey.FilePickerAllFiles))
                {
                    Patterns =
                    [
                        "*.*"
                    ]
                },
                _skriptFileType
            ]
        };
        return dialogOptions;
    }

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return TopLevel.GetTopLevel(desktop.MainWindow)?.StorageProvider;
        }

        return null;
    }
}