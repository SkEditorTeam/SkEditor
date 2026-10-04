using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;
using SkEditor.Utilities;

namespace SkEditor.Services;

public class DocumentationService(HttpClient httpClient) : IDocumentationService
{
    private readonly string _docsPath = Path.Combine(AppPaths.LocalData, "Documentation");

    public async Task<Stream> GetDocumentationStream(string fileName, string url, CancellationToken ct = default)
    {
        string filePath = Path.Combine(_docsPath, fileName);
        Directory.CreateDirectory(_docsPath);

        if (!File.Exists(filePath))
        {
            await DownloadDocumentation(fileName, url, ct);
        }

        FileStreamOptions options = new()
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan
        };

        return new FileStream(filePath, options);
    }

    public async Task<bool> DownloadDocumentation(string fileName, string url, CancellationToken ct = default)
    {
        string? tempFilePath = null;
        try
        {
            string filePath = Path.Combine(_docsPath, fileName);
            string eTagFilePath = Path.ChangeExtension(filePath, ".etag");

            using HttpRequestMessage request = new(HttpMethod.Get, url);

            if (File.Exists(filePath) && File.Exists(eTagFilePath))
            {
                string savedEtag = await File.ReadAllTextAsync(eTagFilePath, ct);
                if (EntityTagHeaderValue.TryParse(savedEtag, out EntityTagHeaderValue? parsedEtag))
                {
                    request.Headers.IfNoneMatch.Add(parsedEtag);
                }
            }

            using HttpResponseMessage response =
                await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return true;
            }

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            await using Stream networkStream = await response.Content.ReadAsStreamAsync(ct);
            FileStreamOptions fileOptions = new()
            {
                Mode = FileMode.Create,
                Access = FileAccess.Write,
                Share = FileShare.None,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            };

            tempFilePath = Path.Combine(_docsPath, $"temp_{Guid.NewGuid():N}.sk");
            await using (FileStream fileStream = new(tempFilePath, fileOptions))
            {
                await networkStream.CopyToAsync(fileStream, ct);
            }

            File.Move(tempFilePath, filePath, true);
            if (response.Headers.ETag is { } eTag)
            {
                await File.WriteAllTextAsync(eTagFilePath, eTag.ToString(), ct);
            }
            else if (File.Exists(eTagFilePath))
            {
                File.Delete(eTagFilePath);
            }
        }
        catch
        {
            return false;
        }
        finally
        {
            if (tempFilePath != null && File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch
                {
                    // ignore
                }
            }
        }

        return true;
    }

    public void DeleteCache(string fileName)
    {
        string filePath = Path.Combine(_docsPath, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}