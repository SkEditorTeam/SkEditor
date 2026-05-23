using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SkEditor.Models.Tabs;

namespace SkEditor.DataTemplates;

public class TabViewLocator : IDataTemplate
{
    private readonly Dictionary<Tab, Control> _viewCache = new();
    private readonly Dictionary<Type, Func<Tab, Control>> _viewFactories = new();

    public Control Build(object? data)
    {
        if (data is null)
        {
            return new TextBlock { Text = "Null data" };
        }

        if (data is not Tab tab)
        {
            return new TextBlock { Text = "Invalid data type" };
        }

        if (_viewCache.TryGetValue(tab, out Control? cachedView))
        {
            return cachedView;
        }

        if (!_viewFactories.TryGetValue(tab.GetType(), out Func<Tab, Control>? factory))
        {
            return new TextBlock { Text = $"No registered view for {data.GetType().Name}" };
        }

        Control view = factory(tab);
        if (tab.IsCacheable)
        {
            _viewCache[tab] = view;
        }

        return view;
    }

    public bool Match(object? data)
    {
        return data is Tab;
    }

    public void Register<TTab>(Func<Tab, Control> factory) where TTab : Tab
    {
        _viewFactories[typeof(TTab)] = factory;
    }

    // TODO: call it!
    public void RemoveFromCache(Tab tab)
    {
        _viewCache.Remove(tab);
    }
}