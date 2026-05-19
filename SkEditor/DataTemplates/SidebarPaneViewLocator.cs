using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace SkEditor.DataTemplates;

public class SidebarPaneViewLocator : IRecyclingDataTemplate
{
    private readonly Dictionary<Type, Control> _built = new();
    private readonly Dictionary<Type, Func<Control>> _registry = new();

    public Control? Build(object? data)
    {
        return Build(data, null);
    }

    public Control? Build(object? data, Control? existing)
    {
        if (data is null)
            return null;

        if (!_registry.TryGetValue(data.GetType(), out Func<Control>? factory))
        {
            return new TextBlock { Text = $"No view registered for {data.GetType().Name}" };
        }

        if (!_built.TryGetValue(data.GetType(), out Control? control))
        {
            control = factory();
            _built[data.GetType()] = control;
        }

        control.DataContext = data;
        return control;
    }

    public bool Match(object? data)
    {
        return data is not null && _registry.ContainsKey(data.GetType());
    }

    public void Register<TViewModel, TView>()
        where TViewModel : class
        where TView : Control, new()
    {
        _registry[typeof(TViewModel)] = () => new TView();
    }
}