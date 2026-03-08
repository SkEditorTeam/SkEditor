using System.Threading.Tasks;
using SkEditor.Models.Projects;

namespace SkEditor.Contracts.Models;

public interface IFileSystem
{
    FileSystemNode? Root { get; }
    Task<bool> Load();
}