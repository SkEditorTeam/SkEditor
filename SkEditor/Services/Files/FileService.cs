using System;
using System.Linq;
using System.Text.RegularExpressions;
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

        // FileTab newFileTab = fileTabFactory.CreateNewFile(header.Value);
        const string scriptFolder = @"C:\Users\Notro\Documents\Skrypty";
        string[] existingFiles = System.IO.Directory.GetFiles(scriptFolder, "*.sk");
        
        Random random = new();
        
        FileTab newFileTab = fileTabFactory.CreateFromFile(existingFiles[random.Next(existingFiles.Length)]);
        _ = newFileTab.LoadContentAsync();
        
        tabService.OpenTab(newFileTab);
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