using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Documentation.Models;

namespace SkEditor.Documentation.Providers;

public interface IDocumentationProvider
{
    string Name { get; }
    string FileName { get; }
    string GetDownloadUrl();

    Task<IReadOnlyList<DocumentationEntry>> GetDocumentationEntriesAsync(Stream jsonStream, CancellationToken ct);
}