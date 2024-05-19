using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Docs.SkriptHub;

public class SkriptHubProvider : IDocProvider
{
    public DocProvider Provider => DocProvider.SkriptHub;
    public Task<IDocumentationEntry> FetchElement(string id)
    {
        throw new System.NotImplementedException();
    }

    public List<string> CanSearch(SearchData searchData)
    {
        throw new System.NotImplementedException();
    }

    public Task<List<IDocumentationEntry>> Search(SearchData searchData)
    {
        throw new System.NotImplementedException();
    }
}