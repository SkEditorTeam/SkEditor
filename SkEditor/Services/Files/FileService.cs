using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Models.Files;
using SkEditor.Models.Notifications;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;

namespace SkEditor.Services.Files;

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

        FileTab newFileTab = fileTabFactory.CreateNewFile(header.Value);
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

        FileTab? existingTab = tabs.Tabs.OfType<FileTab>().FirstOrDefault(tab => tab.File.Path == file.Path);

        if (existingTab is not null)
        {
            tabs.SelectedTab = existingTab;
            return;
        }

        FileTab fileTab = fileTabFactory.CreateFromFile(file);

        Task<bool> fullLoadTask = LoadAndReadFileAsync(fileTab);

        Task timeoutTask = Task.Delay(100);

        Task completedTask = await Task.WhenAny(fullLoadTask, timeoutTask);

        if (completedTask == fullLoadTask)
        {
            bool isSuccess = await fullLoadTask;
            if (isSuccess)
            {
                fileTab.IsLoading = false;
                tabs.OpenTab(fileTab);
            }
        }
        else
        {
            fileTab.IsLoading = true;
            tabs.OpenTab(fileTab);

            bool isSuccess = await fullLoadTask;

            fileTab.IsLoading = false;

            if (!isSuccess)
            {
                tabs.CloseTab(fileTab);
            }
        }
    }

    public async Task SaveFileAsync()
    {
        if (tabs.SelectedTab is FileTab fileTab)
        {
            await fileTab.SaveAsync();
        }
    }

    public async Task SaveAsFileAsync()
    {
        if (tabs.SelectedTab is FileTab fileTab)
        {
            await fileTab.SaveAsAsync();
        }
    }

    public async Task SaveAllAsync()
    {
        IEnumerable<FileTab> fileTabs = tabs.Tabs.OfType<FileTab>();

        foreach (FileTab fileTab in fileTabs)
        {
            await fileTab.SaveAsync();
        }
    }

    private async Task<bool> LoadAndReadFileAsync(FileTab fileTab)
    {
        try
        {
            return await fileTab.LoadContentAsync();
        }
        catch (UnauthorizedAccessException)
        {
            Notification notification = Notification
                .Create(TranslatableString.FromRawString("Load Failed"))
                .WithMessage(TranslatableString.FromRawString("Unable to load the file. You do not have permission."))
                .WithType(NotificationType.Error);

            notifications.Show(notification);
            return false;
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to load file.");
            Notification notification = Notification
                .Create(TranslatableString.FromRawString("Load Failed"))
                .WithMessage(TranslatableString.FromRawString("An error occurred while loading the file."))
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

        HashSet<int> existingNumbers = tabs.Tabs
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