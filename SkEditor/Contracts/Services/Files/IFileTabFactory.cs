using SkEditor.Models.Tabs;

namespace SkEditor.Contracts.Services.Files;

public interface IFileTabFactory
{
    FileTab CreateFromFile(string path);
    FileTab CreateNewFile(string name);
}