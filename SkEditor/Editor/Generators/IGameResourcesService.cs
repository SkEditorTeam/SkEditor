using System.Threading;
using System.Threading.Tasks;
using SkEditor.Editor.Generators.Models;

namespace SkEditor.Editor.Generators;

public interface IGameResourcesService
{
    Task<bool> DownloadResources(CancellationToken ct);
    
    Task<Item[]?> GetItemsAsync(CancellationToken ct);
}