using System.Threading.Tasks;

namespace SkEditor.Contracts.Services.Files;

public interface IFileService
{
    void NewFile();
    Task OpenFile(string path);
}