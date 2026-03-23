using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Tabs;

namespace SkEditor.Services.Files;

public class FileService(ITabService tabService) : IFileService
{
    public void NewFile()
    {
        Tab newFileTab = new EditorTab("New file 1", string.Empty);
        tabService.OpenTab(newFileTab);
    }
}