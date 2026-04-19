using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaEdit;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Features;
using SkEditor.Models.Tabs;

namespace SkEditor.Services.Files.Features;

public class FileFeatureService(IServiceProvider serviceProvider) : IFileFeatureService
{
    private readonly Dictionary<FileTab, List<IEditorFeature>> _attachedFeatures = [];

    public void OnTabOpened(FileTab tab)
    {
        List<IEditorFeature> features = serviceProvider.GetServices<IEditorFeature>()
            .Where(f => f.CanAttach(tab))
            .ToList();

        _attachedFeatures[tab] = features;

        foreach (IEditorFeature feature in features)
        {
            feature.Attach(tab);
        }
    }

    public void OnTabClosed(FileTab tab)
    {
        if (!_attachedFeatures.TryGetValue(tab, out var features))
        {
            return;
        }

        foreach (var feature in features)
        {
            feature.Detach(tab);
        }

        _attachedFeatures.Remove(tab);
    }

    public IEnumerable<IEditorFeature> GetFeaturesForTab(FileTab tab)
    {
        return _attachedFeatures.TryGetValue(tab, out var features) ? features : [];
    }

    public void OnViewLoaded(FileTab tab, TextEditor editor)
    {
        if (!_attachedFeatures.TryGetValue(tab, out var features)) return;

        foreach (var feature in features)
        {
            feature.OnViewAttached(tab, editor);
        }
    }

    public void OnViewUnloaded(FileTab tab, TextEditor editor)
    {
        if (!_attachedFeatures.TryGetValue(tab, out var features)) return;
        foreach (var feature in features)
        {
            feature.OnViewDetached(tab, editor);
        }
    }
}