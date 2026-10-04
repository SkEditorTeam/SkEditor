using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Contracts.Services;

/// <summary>
///     Handles documentation retrieval, download and caching
/// </summary>
public interface IDocumentationService
{
    /// <summary>
    ///     Gets a stream for the documentation file. If the file doesn't exist, it will be downloaded first.
    /// </summary>
    Task<Stream> GetDocumentationStream(string fileName, string url, CancellationToken ct = default);

    /// <summary>
    ///     Downloads the documentation file from the specified URL and saves it to the local cache.
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    /// <remarks>Tries to use ETag to avoid unnecessary downloads.</remarks>
    Task<bool> DownloadDocumentation(string fileName, string url, CancellationToken ct = default);

    /// <summary>
    ///     Deletes the cached documentation file.
    /// </summary>
    void DeleteCache(string fileName);
}