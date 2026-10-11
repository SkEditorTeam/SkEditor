using SkEditor.Models.Files;
using SkEditor.Models.Session;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Contracts.Services.Files;

public interface IFileTabFactory
{
    FileTabViewModel CreateFromFile(BaseFile file);
    FileTabViewModel CreateFromSession(SessionStateFile file);
    FileTabViewModel CreateNewFile(string name);
}