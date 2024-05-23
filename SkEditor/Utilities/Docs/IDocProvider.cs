using SkEditor.Utilities.Docs.Local;
using SkEditor.Utilities.Docs.SkriptHub;
using SkEditor.Utilities.Docs.SkriptMC;
using SkEditor.Utilities.Docs.SkUnity;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;

namespace SkEditor.Utilities.Docs;

public interface IDocProvider
{
    public static readonly Dictionary<DocProvider, IDocProvider> Providers = new()
    {
        { DocProvider.SkUnity, new SkUnityProvider()},
        { DocProvider.SkriptHub, new SkriptHubProvider()},
        { DocProvider.SkriptMC, new SkriptMCProvider()},
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

    public Task<Color?> GetAddonColor(string addonName);
    
    public IconSource Icon { get; }
}