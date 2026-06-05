using System.Collections.Generic;
using System.Threading.Tasks;
using SkEditor.Models.Documentation;

namespace SkEditor.Utilities.Docs.Providers;

public interface IDocumentationProvider
{
    string Name { get; }
    Task<IReadOnlyList<DocumentationEntry>> GetDocumentationEntriesAsync();
}