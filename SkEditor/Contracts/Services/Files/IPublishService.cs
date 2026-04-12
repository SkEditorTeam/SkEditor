using System.Threading.Tasks;
using SkEditor.Models.Integrations;

namespace SkEditor.Contracts.Services.Files;

public interface IPublishService
{
    public Task<string?> PublishCode(string code, string apiKey, PublishType type, string language, bool isAnonymous);
}