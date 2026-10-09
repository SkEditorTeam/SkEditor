using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.DataTemplates;

public class TabViewLocator : IDataTemplate
{
    private readonly Dictionary<Type, Func<Control>> _registry = new();
    private readonly Dictionary<ITab, Control> _viewCache = new();

    public Control Build(object? data)
    {
        if (data is not ITab tabVm)
        {
            return new TextBlock { Text = "Invalid data type" };
        }

        if (_viewCache.TryGetValue(tabVm, out Control? cachedView))
        {
            return cachedView;
        }

        if (!_registry.TryGetValue(tabVm.GetType(), out Func<Control>? factory))
        {
            return new TextBlock { Text = $"No registered view for {tabVm.GetType().Name}" };
        }

        Control view = factory();
        view.DataContext = tabVm;

        if (tabVm.IsCacheable)
        {
            _viewCache[tabVm] = view;
        }

        return view;
    }

    public bool Match(object? data)
    {
        return data is ITab;
    }

    public void Register<TViewModel, TView>()
        where TViewModel : ITab
        where TView : Control, new()
    {
        _registry[typeof(TViewModel)] = () => new TView();
    }

    public void RemoveFromCache(ITab tabVm)
    {
        _viewCache.Remove(tabVm);
    }
}