using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.DataTemplates;
using SkEditor.ViewModels.Tabs.Capabilities;
using TabViewModelBase = SkEditor.ViewModels.Tabs.TabViewModelBase;

namespace SkEditor.Services.Files;

public class TabService(IFileFeatureService features, TabViewLocator tabViewLocator, INotificationService notifs)
    : ITabService
{
    public ObservableCollection<TabViewModelBase> MainTabs { get; } = [];
    public ObservableCollection<TabViewModelBase> SplitTabs { get; } = [];

    public IEnumerable<TabViewModelBase> AllTabs => MainTabs.Concat(SplitTabs);

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

            if (value != null)
            {
                if (MainTabs.Contains(value))
                {
                    SelectedMainTab = value;
                }
                else if (SplitTabs.Contains(value))
                {
                    SelectedSplitTab = value;
                }
            }

            SelectedTabChanged?.Invoke(value);
        }
    }

    public TabViewModelBase? SelectedMainTab
    {
        get;
        set
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            if (value != null)
            {
                SelectedTab = value;
            }
        }
    }

    public TabViewModelBase? SelectedSplitTab
    {
        get;
        set
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            if (value != null)
            {
                SelectedTab = value;
            }
        }
    }

    public event Action<TabViewModelBase?>? SelectedTabChanged;
    public event Action<TabViewModelBase>? TabClosed;

    public void SwitchTab(int index)
    {
        if (SelectedTab is null)
        {
            return;
        }

        ObservableCollection<TabViewModelBase> activeList = SplitTabs.Contains(SelectedTab) ? SplitTabs : MainTabs;

        if (index < 0 || index >= activeList.Count)
        {
            return;
        }

        SelectedTab = activeList[index];
    }

    public void OpenTab(TabViewModelBase tab)
    {
        tab.CloseRequested += OnTabCloseRequested;

        if (tab is IFocusableTab focusableTab)
        {
            focusableTab.FocusRequested += OnFocusRequested;
        }

        if (tab is IFeatureTab featureTab)
        {
            features.OnTabOpened(featureTab);
        }

        MainTabs.Add(tab);
        SelectedTab = tab;
        SelectedMainTab = tab;
    }

    public void CloseTab(TabViewModelBase tab)
    {
        tab.CloseRequested -= OnTabCloseRequested;

        if (tab is IFocusableTab focusableTab)
        {
            focusableTab.FocusRequested -= OnFocusRequested;
        }

        bool isMain = MainTabs.Contains(tab);
        ObservableCollection<TabViewModelBase> targetList = isMain ? MainTabs : SplitTabs;

        targetList.Remove(tab);
        tabViewLocator.RemoveFromCache(tab);

        if (SelectedTab == tab)
        {
            TabViewModelBase? nextTab = targetList.LastOrDefault() ??
                                        (isMain ? SplitTabs.LastOrDefault() : MainTabs.LastOrDefault());

            SelectedTab = nextTab;

            if (isMain)
            {
                SelectedMainTab = nextTab;
            }
            else
            {
                SelectedSplitTab = nextTab;
            }
        }

        if (tab is IFeatureTab featureTab)
        {
            features.OnTabClosed(featureTab);
        }

        TabClosed?.Invoke(tab);
    }

    public void SplitTab(TabViewModelBase tab)
    {
        if (!MainTabs.Contains(tab))
        {
            return;
        }

        MainTabs.Remove(tab);
        SplitTabs.Add(tab);
        SelectedSplitTab = tab;
    }

    public void MergeTab(TabViewModelBase tab)
    {
        if (!SplitTabs.Contains(tab))
        {
            return;
        }

        SplitTabs.Remove(tab);
        MainTabs.Add(tab);
        SelectedMainTab = tab;
    }

    public void OpenUniqueTab<T>(Func<T> factory, bool allowMultiple) where T : TabViewModelBase
    {
        if (!allowMultiple && AllTabs.OfType<T>().FirstOrDefault() is { } existing)
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

    private void OnFocusRequested(object? sender, EventArgs e)
    {
        if (sender is TabViewModelBase tab)
        {
            SelectedTab = tab;
        }
    }
}