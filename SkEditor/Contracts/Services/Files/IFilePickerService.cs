using System.Threading.Tasks;

namespace SkEditor.Contracts.Services.Files;

public interface IFilePickerService
{
    Task<string[]?> PickFilesAsync();
    Task<string?> SaveFileAsync();
}