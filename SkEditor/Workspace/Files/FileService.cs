using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Workspace.Files;

public class FileService(
    ITabService tabs,
    ITranslationService translation,
    IFileTabFactory fileTabFactory,
    IFilePickerService filePicker,
    IRecentFileService recentFiles,
    INotificationService notifications,
    ILogService logs)
    : IFileService
{
    public void NewFile()
    {
        TranslatableString header =
            translation.GetObservable(TranslationKey.NewFileNameFormat, GetNewFileNumber().ToString());

        FileTabViewModel newFileTab = fileTabFactory.CreateNewFile(header.Value);
        tabs.OpenTab(newFileTab);
    }

    public async Task OpenFileAsync()
    {
        string[]? paths = await filePicker.PickFilesAsync();
        if (paths is null)
        {
            return;
        }

        foreach (string path in paths)
        {
            await OpenFileAsync(new LocalFile(path));
        }
    }

    public async Task OpenFileAsync(BaseFile file)
    {
        if (file is LocalFile && file.Name is not null && file.Path is not null)
        {
            recentFiles.AddRecentItem(file.Name, file.Path, false, DateTime.Now);
        }

        FileTabViewModel? existingTab =
            tabs.AllTabs.OfType<FileTabViewModel>().FirstOrDefault(tab => tab.File.Path == file.Path);

        if (existingTab is not null)
        {
            tabs.SelectedTab = existingTab;
            return;
        }

        FileTabViewModel fileTab = fileTabFactory.CreateFromFile(file);
        await LoadAndOpenFileTabAsync(fileTab);
    }

    public async Task LoadAndOpenFileTabAsync(FileTabViewModel fileTab, bool select = true)
    {
        Task<bool> fullLoadTask = LoadAndReadFileAsync(fileTab);

        Task timeoutTask = Task.Delay(100);

        Task completedTask = await Task.WhenAny(fullLoadTask, timeoutTask);

        if (completedTask == fullLoadTask)
        {
            bool isSuccess = await fullLoadTask;
            if (isSuccess)
            {
                fileTab.IsLoading = false;
                tabs.OpenTab(fileTab, select);
            }
        }
        else
        {
            fileTab.IsLoading = true;
            tabs.OpenTab(fileTab, select);

            bool isSuccess = await fullLoadTask;

            fileTab.IsLoading = false;

            if (!isSuccess)
            {
                tabs.CloseTab(fileTab);
            }
        }
    }

    public void UpdateFileDetails(string oldPath, string newName, string newPath)
    {
        FileTabViewModel? fileTab = tabs.AllTabs
            .OfType<FileTabViewModel>()
            .FirstOrDefault(tab => tab.File.Path == oldPath);

        fileTab?.File.Name = newName;
        fileTab?.File.Path = newPath;
        fileTab?.UpdateHeader();
    }

    public void NotifyFileDelete(string filePath)
    {
        FileTabViewModel? fileTab = tabs.AllTabs
            .OfType<FileTabViewModel>()
            .FirstOrDefault(tab => tab.File.Path == filePath);

        fileTab?.IsSaved = false;
        fileTab?.File.Path = null;
        fileTab?.Document.UndoStack.DiscardOriginalFileMarker();
    }

    public async Task SaveFileAsync()
    {
        if (tabs.SelectedTab is ISaveableTab saveableTab)
        {
            await saveableTab.SaveAsync();
        }
    }

    public async Task SaveAsFileAsync()
    {
        if (tabs.SelectedTab is ISaveableTab saveableTab)
        {
            await saveableTab.SaveAsAsync();
        }
    }

    public async Task SaveAllAsync()
    {
        IEnumerable<FileTabViewModel> fileTabs = tabs.AllTabs.OfType<FileTabViewModel>();

        foreach (FileTabViewModel fileTab in fileTabs)
        {
            await fileTab.SaveAsync();
        }
    }

    public async Task<bool> LoadAndReadFileAsync(FileTabViewModel fileTab)
    {
        try
        {
            return await fileTab.LoadContentAsync();
        }
        catch (UnauthorizedAccessException)
        {
            Notification notification = Notification
                .Create(TranslationKey.FileLoadFailedTitle)
                .WithMessage(TranslationKey.FileLoadFailedPermissionMessage)
                .WithType(NotificationType.Error);

            notifications.Show(notification);
            return false;
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to load file.");
            Notification notification = Notification
                .Create(TranslationKey.FileLoadFailedTitle)
                .WithMessage(TranslationKey.FileLoadFailedGenericMessage)
                .WithType(NotificationType.Error);

            notifications.Show(notification);
            return false;
        }
    }

    private int GetNewFileNumber()
    {
        string template = translation.Get(TranslationKey.NewFileNameFormat);
        string pattern = Regex.Escape(template).Replace(@"\{0}", @"(\d+)");
        Regex regex = new(pattern);

        HashSet<int> existingNumbers = tabs.AllTabs
            .Select(tab => regex.Match(tab.Header.Value))
            .Where(m => m.Success)
            .Select(m => int.Parse(m.Groups[1].Value))
            .ToHashSet();

        int number = 1;
        while (existingNumbers.Contains(number))
        {
            number++;
        }

        return number;
    }
}