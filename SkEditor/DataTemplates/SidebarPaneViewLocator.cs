using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace SkEditor.DataTemplates;

public class SidebarPaneViewLocator : IDataTemplate
{
    private readonly Dictionary<Type, Func<Control>> _registry = new();

    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        if (_registry.TryGetValue(data.GetType(), out Func<Control>? factory))
        {
            return factory();
        }

        return new TextBlock { Text = $"No view registered for {data.GetType().Name}" };
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