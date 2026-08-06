using System;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Models.Files;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Services.Files;

public class FileTabFactory(
    AppSettings appSettings,
    ExperimentsConfig experiments,
    IDialogService dialogService,
    IFilePickerService filePicker,
    IFileFeatureService features,
    IRecentFileService recentFiles,
    INotificationService notifications,
    ILogService logs) : IFileTabFactory
{
    public FileTabViewModel CreateFromFile(BaseFile file)
    {
        if (file.Name is null)
        {
            throw new ArgumentException("File name cannot be null in this method");
        }

        TranslatableString header = TranslatableString.FromRawString(file.Name);

        return new FileTabViewModel(header, appSettings, experiments, dialogService, notifications, filePicker,
            features, recentFiles, logs)
        {
            File = file
        };
    }

    public FileTabViewModel CreateNewFile(string name)
    {
        TranslatableString header = TranslatableString.FromRawString(name);

        return new FileTabViewModel(header, appSettings, experiments, dialogService, notifications, filePicker,
            features, recentFiles, logs)
        {
            File = new LocalFile(name: name),
            IsSaved = true
        };
    }
}