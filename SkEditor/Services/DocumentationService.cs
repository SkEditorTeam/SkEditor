using System;
using System.IO;
using System.Net.Http;
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

    public async Task DownloadDocumentation(string fileName, string url, CancellationToken ct = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        using HttpResponseMessage response =
            await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using Stream networkStream = await response.Content.ReadAsStreamAsync(ct);
        FileStreamOptions fileOptions = new()
        {
            Mode = FileMode.Create,
            Access = FileAccess.Write,
            Share = FileShare.None,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan
        };

        string tempFilePath = Path.Combine(_docsPath, $"temp_{Guid.NewGuid():N}.sk");
        await using (FileStream fileStream = new(tempFilePath, fileOptions))
        {
            await networkStream.CopyToAsync(fileStream, ct);
        }

        string filePath = Path.Combine(_docsPath, fileName);
        File.Move(tempFilePath, filePath, true);
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