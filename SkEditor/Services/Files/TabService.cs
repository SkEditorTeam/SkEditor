using System;
using System.Collections.ObjectModel;
using System.Linq;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.DataTemplates;
using SkEditor.ViewModels.Tabs.Capabilities;
using TabViewModelBase = SkEditor.ViewModels.Tabs.TabViewModelBase;

namespace SkEditor.Services.Files;

public class TabService(IFileFeatureService features, TabViewLocator tabViewLocator) : ITabService
{
    public ObservableCollection<TabViewModelBase> Tabs { get; } = [];

    public TabViewModelBase? SelectedTab
    {
        get;
        set
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            SelectedTabChanged?.Invoke(value);
        }
    }

    public event Action<TabViewModelBase?>? SelectedTabChanged;
    public event Action<TabViewModelBase>? TabClosed;

    public void SwitchTab(int index)
    {
        if (index < 0 || index >= Tabs.Count)
        {
            return;
        }

        SelectedTab = Tabs[index];
    }

    public void OpenTab(TabViewModelBase tab)
    {
        tab.CloseRequested += OnTabCloseRequested;

        if (tab is IFeatureTab featureTab)
        {
            features.OnTabOpened(featureTab);
        }

        Tabs.Add(tab);
        SelectedTab = tab;
    }

    public void CloseTab(TabViewModelBase tab)
    {
        tab.CloseRequested -= OnTabCloseRequested;
        Tabs.Remove(tab);

        tabViewLocator.RemoveFromCache(tab);

        if (SelectedTab == tab)
        {
            SelectedTab = Tabs.LastOrDefault();
        }

        if (tab is IFeatureTab featureTab)
        {
            features.OnTabClosed(featureTab);
        }

        TabClosed?.Invoke(tab);
    }

    public void OpenUniqueTab<T>(Func<T> factory, bool allowMultiple) where T : TabViewModelBase
    {
        if (!allowMultiple && Tabs.OfType<T>().FirstOrDefault() is { } existing)
        {
            SelectedTab = existing;
            return;
        }

        OpenTab(factory());
    }

    private void OnTabCloseRequested(object? sender, EventArgs e)
    {
        if (sender is TabViewModelBase tab)
        {
            CloseTab(tab);
        }
    }
}