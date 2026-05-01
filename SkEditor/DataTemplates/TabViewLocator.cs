using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SkEditor.Models.Tabs;

namespace SkEditor.DataTemplates;

public class TabViewLocator : IDataTemplate
{
    private readonly Dictionary<object, Control> _viewCache = new();
    private readonly Dictionary<Type, Func<object, Control>> _viewFactories = new();

    public Control Build(object? data)
    {
        if (data is null)
        {
            return new TextBlock { Text = "Null data" };
        }

        if (_viewCache.TryGetValue(data, out Control? cachedView))
            return cachedView;

        if (!_viewFactories.TryGetValue(data.GetType(), out Func<object, Control>? factory))
        {
            return new TextBlock { Text = $"No registered view for {data.GetType().Name}" };
        }

        Control view = factory(data);
        _viewCache[data] = view;
        return view;
    }

    public bool Match(object? data)
    {
        return data is Tab;
    }

    public void Register<TTab>(Func<object, Control> factory) where TTab : Tab
    {
        _viewFactories[typeof(TTab)] = factory;
    }
}