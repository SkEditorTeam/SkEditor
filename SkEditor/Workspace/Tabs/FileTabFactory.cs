using System;
using System.IO;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Extensions;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Core.System;
using SkEditor.Editor.Features;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Session.Models;
using AppSettings = SkEditor.Configuration.Configs.AppSettings;
using ExperimentsConfig = SkEditor.Configuration.Configs.ExperimentsConfig;
using FileTabViewModel = SkEditor.Editor.UI.Tabs.FileTabViewModel;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Workspace.Tabs;

public class FileTabFactory(
    AppSettings appSettings,
    ExperimentsConfig experiments,
    IDialogService dialogService,
    IFilePickerService filePicker,
    IFileFeatureService features,
    IRecentFileService recentFiles,
    INotificationService notifications,
    ILinkService links,
    IClipboardService clipboard,
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
            features, recentFiles, links, clipboard, logs)
        {
            File = file
        };
    }

    public FileTabViewModel CreateFromSession(SessionStateFile file)
    {
        TranslatableString? header;
        if (file.Header is { } fileHeader)
        {
            header = fileHeader.ToTranslatable();
        }
        else if (file.Path is not null)
        {
            string fileName = Path.GetFileName(file.Path);
            header = fileName.ToTranslatable();
        }
        else
        {
            throw new InvalidOperationException("The session state file is missing both header and path!");
        }

        return new FileTabViewModel(header, appSettings, experiments, dialogService, notifications, filePicker,
            features, recentFiles, links, clipboard, logs)
        {
            File = new LocalFile(file.Path, file.Header),
            IsSaved = file.IsSaved
        };
    }

    public FileTabViewModel CreateNewFile(string name)
    {
        TranslatableString header = TranslatableString.FromRawString(name);

        return new FileTabViewModel(header, appSettings, experiments, dialogService, notifications, filePicker,
            features, recentFiles, links, clipboard, logs)
        {
            File = new LocalFile(name: name),
            IsSaved = true
        };
    }
}