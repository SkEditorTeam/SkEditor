using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Contracts.Services;

public interface IDocumentationService
{
    Task<Stream> GetDocumentationStream(string fileName, string url, CancellationToken ct = default);
}