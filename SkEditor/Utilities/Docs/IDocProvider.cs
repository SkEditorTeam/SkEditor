using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Docs.Local;
using SkEditor.Utilities.Docs.SkriptHub;
using SkEditor.Utilities.Docs.SkriptMC;
using SkEditor.Utilities.Docs.SkUnity;

namespace SkEditor.Utilities.Docs;

public interface IDocProvider
{
    public static readonly Dictionary<DocProvider, IDocProvider> Providers = new()
    {
        { DocProvider.SkriptHub, new SkriptHubProvider() },
        { DocProvider.skUnity, new SkUnityProvider() },
        { DocProvider.SkriptMC, new SkriptMCProvider() },
        { DocProvider.Local, new LocalProvider() }
    };

    public DocProvider Provider { get; }
    public string Name => Provider.ToString();

    public bool NeedsToLoadExamples { get; }

    public bool HasAddons { get; }

    public IconSource Icon { get; }

    public Task<IDocumentationEntry?> FetchElement(string id);
    public Task<List<IDocumentationEntry>> Search(SearchData searchData);

    public List<string> CanSearch(SearchData searchData);
    public bool IsAvailable();
    public Task<List<IDocumentationExample>> FetchExamples(IDocumentationEntry entry);
    public Task<List<string>> GetAddons();

    public Task<Color?> GetAddonColor(string addonName);
    public string? GetLink(IDocumentationEntry entry);
}