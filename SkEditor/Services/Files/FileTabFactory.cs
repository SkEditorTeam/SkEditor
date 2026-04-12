using System.IO;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Files;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Services.Files;

public class FileTabFactory(AppSettings appSettings, INotificationService notificationService) : IFileTabFactory
{
    public FileTab CreateFromFile(string path)
    {
        string fileName = Path.GetFileName(path);
        var header = TranslatableString.FromRawString(fileName);

        return new FileTab(header, appSettings, notificationService)
        {
            File = new LocalFile(fileName, path)
        };
    }

    public FileTab CreateNewFile(string name)
    {
        TranslatableString header = TranslatableString.FromRawString(name);

        return new FileTab(header, appSettings, notificationService)
        {
            File = new LocalFile(name)
        };
    }
}