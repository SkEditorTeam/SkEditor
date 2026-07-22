using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.DataTemplates;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Localization;
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

        if (SelectedTab is not null && SplitTabs.Contains(SelectedTab))
        {
            SplitTabs.Add(tab);
            SelectedSplitTab = tab;
        }
        else
        {
            MainTabs.Add(tab);
            SelectedMainTab = tab;
        }
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

        if (MainTabs.Count == 0 && SplitTabs.Count > 0)
        {
            List<TabViewModelBase> itemsToMove = SplitTabs.ToList();
            SplitTabs.Clear();

            foreach (TabViewModelBase item in itemsToMove)
            {
                MainTabs.Add(item);
            }
        }

        if (SelectedTab == tab)
        {
            TabViewModelBase? nextTab = MainTabs.LastOrDefault() ?? SplitTabs.LastOrDefault();
            SelectedTab = nextTab;
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

        if (MainTabs.Count == 1)
        {
            notifs.Show(Notification
                .Create(TranslatableString.FromRawString("Oops!"))
                .WithMessage(TranslatableString.FromRawString("You can't split the only tab open in the main view."))
                .WithType(NotificationType.Warning));

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