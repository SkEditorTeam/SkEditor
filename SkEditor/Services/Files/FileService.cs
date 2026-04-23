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

    public async Task OpenFileAsync(string path)
    {
        FileTab fileTab = fileTabFactory.CreateFromFile(path);
        await fileTab.LoadContentAsync();
        tabs.OpenTab(fileTab);
    }

    public async Task SaveFileAsync()
    {
        if (tabs.SelectedTab is FileTab fileTab)
        {
            if (fileTab.File.Path == null)
            {
                await SaveAsFileAsync();
                return;
            }

            await fileTab.SaveContentAsync();
        }
    }

    public async Task SaveAsFileAsync()
    {
        if (tabs.SelectedTab is FileTab fileTab)
        {
            string? path = await filePicker.SaveFileAsync();
            if (path != null)
            {
                fileTab.File.Path = path;
                fileTab.UpdateHeader();
                await fileTab.SaveContentAsync();
            }
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