using System.Collections.Generic;
using System.Threading.Tasks;
using SkEditor.Utilities.Docs.SkUnity;

namespace SkEditor.Utilities.Docs;

public interface IDocProvider
{
    public static readonly Dictionary<DocProvider, IDocProvider> Providers = new()
    {
        { DocProvider.SkriptHub, new SkUnityProvider()}
    };
    
    public DocProvider Provider { get; }
    public string Name => Provider.ToString();
    
    public Task<IDocumentationEntry> FetchElement(string id);
    public Task<List<IDocumentationEntry>> Search(SearchData searchData);
    
    public List<string> CanSearch(SearchData searchData);

}