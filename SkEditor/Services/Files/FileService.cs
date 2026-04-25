using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;

namespace SkEditor.Services.Files;

public class FileService(
    ITabService tabs,
    ITranslationService translation,
    IFileTabFactory fileTabFactory,
    IFilePickerService filePicker)
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
            await OpenFileAsync(path);
        }
    }

    public async Task OpenFileAsync(string path)
    {
        FileTab? existingTab = tabs.Tabs.OfType<FileTab>().FirstOrDefault(tab => tab.File.Path == path);

        if (existingTab is not null)
        {
            tabs.SelectedTab = existingTab;
            return;
        }

        FileTab fileTab = fileTabFactory.CreateFromFile(path);
        await fileTab.LoadContentAsync();
        tabs.OpenTab(fileTab);
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