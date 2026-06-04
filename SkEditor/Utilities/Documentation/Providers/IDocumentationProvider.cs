using System.Collections.Generic;
using System.Threading.Tasks;
using SkEditor.Models.Documentation;

namespace SkEditor.Utilities.Docs.Providers;

public interface IDocumentationProvider
{
    Task<IReadOnlyList<DocumentationEntry>> GetDocumentationEntriesAsync();
}