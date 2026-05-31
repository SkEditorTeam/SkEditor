using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaEdit;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Features;
using SkEditor.Utilities.Settings;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Services.Files.Features;

public class FileFeatureService : IFileFeatureService
{
    private readonly Dictionary<FileTabViewModel, TextEditor> _activeEditors = [];
    private readonly Dictionary<FileTabViewModel, List<IEditorFeature>> _attachedFeatures = [];
    private readonly IServiceProvider _serviceProvider;

    public FileFeatureService(IServiceProvider serviceProvider, IEnumerable<IConfig> configs)
    {
        _serviceProvider = serviceProvider;

        foreach (IConfig config in configs)
        {
            config.PropertyChanged += (_, _) => UpdateAllFeatures();
        }
    }

    public void OnTabOpened(FileTabViewModel tab)
    {
        List<IEditorFeature> features = CreateAllFeatures()
            .Where(f => f.CanAttach(tab))
            .ToList();

        _attachedFeatures[tab] = features;

        foreach (IEditorFeature feature in features)
        {
            feature.Attach(tab);
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

    public void OnTabClosed(FileTabViewModel tab)
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
    }

    public void OnViewLoaded(FileTabViewModel tab, TextEditor editor)
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

    public void OnViewUnloaded(FileTabViewModel tab, TextEditor editor)
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

    public IEnumerable<IEditorFeature> GetFeaturesForTab(FileTabViewModel tab)
    {
        return _attachedFeatures.TryGetValue(tab, out List<IEditorFeature>? features) ? features : [];
    }

    public void UpdateAllFeatures()
    {
        foreach (FileTabViewModel tab in _attachedFeatures.Keys.ToList())
        {
            UpdateFeatures(tab);
        }
    }

    public void UpdateFeatures(FileTabViewModel tab)
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