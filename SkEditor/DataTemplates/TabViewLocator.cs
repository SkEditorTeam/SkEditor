using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using TabViewModelBase = SkEditor.ViewModels.Tabs.TabViewModelBase;

namespace SkEditor.DataTemplates;

public class TabViewLocator : IDataTemplate
{
    private readonly Dictionary<Type, Control> _viewCache = new();
    private readonly Dictionary<Type, Func<Control>> _registry = new();

    public Control Build(object? data)
    {
        if (data is not TabViewModelBase tabVm)
            return new TextBlock { Text = "Invalid data type" };

        if (_viewCache.TryGetValue(tabVm.GetType(), out var cachedView))
            return cachedView;

        if (!_registry.TryGetValue(tabVm.GetType(), out var factory))
            return new TextBlock { Text = $"No registered view for {tabVm.GetType().Name}" };

        var view = factory();
        view.DataContext = tabVm;

        if (tabVm.IsCacheable)
            _viewCache[tabVm.GetType()] = view;

        return view;
    }

    public bool Match(object? data) => data is TabViewModelBase;

    public void Register<TViewModel, TView>()
        where TViewModel : TabViewModelBase
        where TView : Control, new()
    {
        _registry[typeof(TViewModel)] = () => new TView();
    }
}