using System.Threading.Tasks;

namespace SkEditor.Contracts.Services;

public interface IClipboardService
{
    Task SetTextAsync(string text);
}