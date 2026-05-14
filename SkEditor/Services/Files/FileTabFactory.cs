using System;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Models.Files;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Services.Files;

public class FileTabFactory(
    AppSettings appSettings,
    IDialogService dialogService,
    IFilePickerService filePicker,
    IFileFeatureService features,
    INotificationService notifications,
    ILogService logs) : IFileTabFactory
{
    public FileTab CreateFromFile(BaseFile file)
    {
        if (file.Name is null)
        {
            throw new ArgumentException("File name cannot be null in this method");
        }

        TranslatableString header = TranslatableString.FromRawString(file.Name);

        return new FileTab(header, appSettings, dialogService, notifications, filePicker, features, logs)
        {
            File = file
        };
    }

    public FileTab CreateNewFile(string name)
    {
        TranslatableString header = TranslatableString.FromRawString(name);

        return new FileTab(header, appSettings, dialogService, notifications, filePicker, features, logs)
        {
            File = new LocalFile(name: name),
            IsSaved = true
        };
    }
}