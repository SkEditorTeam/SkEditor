using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;

namespace SkEditor.Services.Files;

public class FileService(ITabService tabService, ITranslationService translation, IFileTabFactory fileTabFactory)
    : IFileService
{
    public void NewFile()
    {
        TranslatableString header =
            translation.GetObservable(TranslationKey.NewFileNameFormat, GetNewFileNumber().ToString());

        FileTab newFileTab = fileTabFactory.CreateNewFile(header.Value);
        tabService.OpenTab(newFileTab);
    }

    public async Task OpenFile(string path)
    {
        FileTab fileTab = fileTabFactory.CreateFromFile(path);
        await fileTab.LoadContentAsync();
        tabService.OpenTab(fileTab);
    }

    public async Task SaveFile()
    {
        if (tabService.SelectedTab is FileTab fileTab)
        {
            await fileTab.SaveContentAsync();
        }
    }

    private int GetNewFileNumber()
    {
        string template = translation.Get(TranslationKey.NewFileNameFormat);
        string pattern = Regex.Escape(template).Replace(@"\{0}", @"(\d+)");
        Regex regex = new(pattern);

        var existingNumbers = tabService.Tabs
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