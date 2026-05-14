using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Models.Files;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;

namespace SkEditor.Services.Files;

public class FileService(
    ITabService tabs,
    ITranslationService translation,
    IFileTabFactory fileTabFactory,
    IFilePickerService filePicker,
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
        FileTab? existingTab = tabs.Tabs.OfType<FileTab>().FirstOrDefault(tab => tab.File.Path == file.Path);

        if (existingTab is not null)
        {
            tabs.SelectedTab = existingTab;
            return;
        }

        Stream stream;
        try
        {
            stream = await file.LoadAsync();
        }
        catch (UnauthorizedAccessException)
        {
            notifications.ShowError(TranslatableString.FromRawString("Load Failed"),
                TranslatableString.FromRawString("Unable to load the file. You do not have permission."));
            return;
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to load file.");
            notifications.ShowError(TranslatableString.FromRawString("Load Failed"),
                TranslatableString.FromRawString("An error occurred while loading the file."));
            return;
        }

        FileTab fileTab = fileTabFactory.CreateFromFile(file);

        Task<bool> loadContentTask = fileTab.LoadContentFromStreamAsync(stream);

        Task timeoutTask = Task.Delay(150);

        Task completedTask = await Task.WhenAny(loadContentTask, timeoutTask);

        if (completedTask == loadContentTask)
        {
            bool isSuccess = await loadContentTask;
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

            bool isSuccess = await loadContentTask;

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

    private int GetNewFileNumber()
    {
        string template = translation.Get(TranslationKey.NewFileNameFormat);
        string pattern = Regex.Escape(template).Replace(@"\{0}", @"(\d+)");
        Regex regex = new(pattern);

        var existingNumbers = tabs.Tabs
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