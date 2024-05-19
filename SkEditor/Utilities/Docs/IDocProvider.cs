using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SkEditor.Utilities.Docs.Local;
using SkEditor.Utilities.Docs.SkriptHub;
using SkEditor.Utilities.Docs.SkUnity;

namespace SkEditor.Utilities.Docs;

public interface IDocProvider
{
    public static readonly Dictionary<DocProvider, IDocProvider> Providers = new()
    {
        { DocProvider.SkUnity, new SkUnityProvider()},
        { DocProvider.SkriptHub, new SkriptHubProvider()},
        { DocProvider.Local, new LocalProvider()}
    };
    
    public DocProvider Provider { get; }
    public string Name => Provider.ToString();
    
    public Task<IDocumentationEntry> FetchElement(string id);
    public Task<List<IDocumentationEntry>> Search(SearchData searchData);
    
    public List<string> CanSearch(SearchData searchData);
    public bool IsAvailable();
    
    public bool NeedsToLoadExamples { get; }
    public Task<List<IDocumentationExample>> FetchExamples(IDocumentationEntry entry);
    
    public bool HasAddons { get; }
    public Task<List<string>> GetAddons();
}