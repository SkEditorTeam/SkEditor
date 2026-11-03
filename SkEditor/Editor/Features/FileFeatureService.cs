using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaEdit;
using Microsoft.Extensions.DependencyInjection;
using R3;
using SkEditor.Configuration.Abstractions;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Editor.Features;

public class FileFeatureService : IFileFeatureService
{
    private readonly Dictionary<IFeatureTab, TextEditor> _activeEditors = [];
    private readonly Dictionary<IFeatureTab, IDisposable> _tabSubscriptions = [];
    private readonly Dictionary<IFeatureTab, List<IEditorFeature>> _attachedFeatures = [];
    private readonly IServiceProvider _serviceProvider;

    public FileFeatureService(IServiceProvider serviceProvider, IEnumerable<IConfig> configs)
    {
        _serviceProvider = serviceProvider;

        foreach (IConfig config in configs)
        {
            config.PropertyChanged += (_, _) => UpdateAllFeatures();
        }
    }

    public void OnTabOpened(IFeatureTab tab)
    {
        List<IEditorFeature> features = CreateAllFeatures()
            .Where(f => f.CanAttach(tab))
            .ToList();

        _attachedFeatures[tab] = features;

        foreach (IEditorFeature feature in features)
        {
            feature.Attach(tab);
        }
        
        if (tab is IFileTab fileTab)
        {
            IDisposable sub = fileTab.File.PathChanged
                .Subscribe(_ => UpdateFeatures(tab));
            
            _tabSubscriptions[tab] = sub;
        }

        if (!_activeEditors.TryGetValue(tab, out TextEditor? editor))
        {
            return;
        }

        foreach (IEditorFeature feature in features)
        {
            feature.OnViewAttached(tab, editor);
        }
    }

    public void OnTabClosed(IFeatureTab tab)
    {
        if (!_attachedFeatures.TryGetValue(tab, out List<IEditorFeature>? features))
        {
            return;
        }

        bool hasView = _activeEditors.TryGetValue(tab, out TextEditor? editor);

        foreach (IEditorFeature feature in features)
        {
            if (hasView)
            {
                feature.OnViewDetached(tab, editor!);
            }

            feature.Detach(tab);
        }

        _attachedFeatures.Remove(tab);
        _activeEditors.Remove(tab);

        if (_tabSubscriptions.Remove(tab, out IDisposable? sub))
        {
            sub.Dispose();
        }
    }

    public void OnViewLoaded(IFeatureTab tab, TextEditor editor)
    {
        _activeEditors[tab] = editor;

        if (!_attachedFeatures.TryGetValue(tab, out List<IEditorFeature>? features))
        {
            return;
        }

        foreach (IEditorFeature feature in features)
        {
            feature.OnViewAttached(tab, editor);
        }
    }

    public void OnViewUnloaded(IFeatureTab tab, TextEditor editor)
    {
        _activeEditors.Remove(tab);

        if (!_attachedFeatures.TryGetValue(tab, out List<IEditorFeature>? features))
        {
            return;
        }

        foreach (IEditorFeature feature in features)
        {
            feature.OnViewDetached(tab, editor);
        }
    }

    public IEnumerable<IEditorFeature> GetFeaturesForTab(IFeatureTab tab)
    {
        return _attachedFeatures.TryGetValue(tab, out List<IEditorFeature>? features) ? features : [];
    }

    public void UpdateAllFeatures()
    {
        foreach (IFeatureTab tab in _attachedFeatures.Keys.ToList())
        {
            UpdateFeatures(tab);
        }
    }

    public void UpdateFeatures(IFeatureTab tab)
    {
        if (!_attachedFeatures.TryGetValue(tab, out List<IEditorFeature>? currentFeatures))
        {
            return;
        }

        bool hasView = _activeEditors.TryGetValue(tab, out TextEditor? editor);

        List<IEditorFeature> newCandidateFeatures = CreateAllFeatures()
            .Where(f => f.CanAttach(tab))
            .ToList();

        HashSet<Type> currentTypes = currentFeatures
            .Select(f => f.GetType())
            .ToHashSet();

        HashSet<Type> desiredTypes = newCandidateFeatures
            .Select(f => f.GetType())
            .ToHashSet();

        IEnumerable<IEditorFeature> toDetach = currentFeatures
            .ToList()
            .Where(feature => !desiredTypes.Contains(feature.GetType()));

        foreach (IEditorFeature feature in toDetach)
        {
            if (hasView)
            {
                feature.OnViewDetached(tab, editor!);
            }

            feature.Detach(tab);
            currentFeatures.Remove(feature);
        }

        IEnumerable<IEditorFeature> toAttach = newCandidateFeatures
            .Where(newFeature => !currentTypes.Contains(newFeature.GetType()));

        foreach (IEditorFeature newFeature in toAttach)
        {
            newFeature.Attach(tab);
            if (hasView)
            {
                newFeature.OnViewAttached(tab, editor!);
            }

            currentFeatures.Add(newFeature);
        }
    }

    private IEnumerable<IEditorFeature> CreateAllFeatures()
    {
        return _serviceProvider.GetServices<IEditorFeature>();
    }
}