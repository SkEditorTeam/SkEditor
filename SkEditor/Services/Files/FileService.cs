using System.Linq;
using System.Text.RegularExpressions;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;

namespace SkEditor.Services.Files;

public class FileService(ITabService tabService) : IFileService
{
    public void NewFile()
    {
        TranslatableString header =
            Translation.GetObservable(TranslationKey.NewFileNameFormat, GetNewFileNumber().ToString());
        Tab newFileTab = new EditorTab(header, string.Empty);
        tabService.OpenTab(newFileTab);
    }

    private int GetNewFileNumber()
    {
        string template = Translation.Get(TranslationKey.NewFileNameFormat);
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