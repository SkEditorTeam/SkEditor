using System.Threading.Tasks;
using SkEditor.Integrations.Publishing.Models;

namespace SkEditor.Integrations.Publishing;

public interface IPublishService
{
    Task<string?> PublishCodeAsync(string code, string apiKey, PublishType type);
}