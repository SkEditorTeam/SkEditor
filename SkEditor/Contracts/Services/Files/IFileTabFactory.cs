using SkEditor.Models.Files;
using SkEditor.Models.Tabs;

namespace SkEditor.Contracts.Services.Files;

public interface IFileTabFactory
{
    FileTab CreateFromFile(BaseFile file);
    FileTab CreateNewFile(string name);
}