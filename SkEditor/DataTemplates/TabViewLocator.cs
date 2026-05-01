using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Models.Tabs;
using SkEditor.Views.Pages;

namespace SkEditor.DataTemplates;

public class TabViewLocator : IDataTemplate
{
    private readonly Dictionary<object, Control> _viewCache = new();
    private readonly Dictionary<Type, Func<object, Control>> _viewFactories = new();

    public TabViewLocator()
    {
        Register<SettingsTab>(tab =>
        {
            SettingsPage page = App.Services.GetRequiredService<SettingsPage>();
            page.DataContext = ((SettingsTab)tab).PageViewModel;
            return page;
        });

        Register<FileTab>(tab => new FileTabView { DataContext = tab });
    }

    public Control Build(object? data)
    {
        if (data is null)
        {
            return new TextBlock { Text = "Null data" };
        }

        if (_viewCache.TryGetValue(data, out Control? cachedView))
        {
            return cachedView;
        }

        Type tabType = data.GetType();
        if (!_viewFactories.TryGetValue(tabType, out Func<object, Control>? factory))
        {
            return new TextBlock { Text = $"No registered view for {tabType.Name}" };
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