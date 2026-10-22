using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SkEditor.Core.Localization;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Workspace.Tabs;

public class FileTabService(
    ITranslationService translation,
    IFilePickerService filePicker,
    IFileTabFactory fileTabFactory,
    IFileService files,
    ITabService tabs,
    IRecentFileService recentFiles)
    : IFileTabService
{
    public ITab NewFile()
    {
        TranslatableString header =
            translation.GetObservable(TranslationKey.NewFileNameFormat, GetNewFileNumber().ToString());

        FileTabViewModel newFileTab = fileTabFactory.CreateNewFile(header.Value);
        return newFileTab;
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

    public async Task LoadAndOpenFileTabAsync(IFileTab fileTab, bool select = true)
    {
        Task<bool> fullLoadTask = files.LoadAndReadFileAsync(fileTab);

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
                tabs.ForceCloseTab(fileTab);
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