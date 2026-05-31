using SkEditor.Models.Files;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Contracts.Services.Files;

public interface IFileTabFactory
{
    FileTabViewModel CreateFromFile(BaseFile file);
    FileTabViewModel CreateNewFile(string name);
}