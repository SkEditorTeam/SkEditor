using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SkEditor.Core.Localization;
using SkEditor.Core.Notifications;
using SkEditor.Editor.Features;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Tabs.Capabilities;
using SkEditor.Workspace.Tabs.Templates;

namespace SkEditor.Workspace.Tabs;

public class TabService(IFileFeatureService features, TabViewLocator tabViewLocator, INotificationService notifs)
    : ITabService
{
    public IEnumerable<ITab> AllTabs => MainTabs.Concat(SplitTabs);
    public ObservableCollection<ITab> MainTabs { get; } = [];

    public ITab? SelectedMainTab
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

    public ITab? SelectedSplitTab
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

    public ITab? SelectedTab
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

    public ObservableCollection<ITab> SplitTabs { get; } = [];

    public void SwitchTab(int index)
    {
        if (SelectedTab is null)
        {
            return;
        }

        ObservableCollection<ITab> activeList = SplitTabs.Contains(SelectedTab) ? SplitTabs : MainTabs;

        if (index < 0 || index >= activeList.Count)
        {
            return;
        }

        SelectedTab = activeList[index];
    }

    public void OpenTab(ITab tab, bool select = true)
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
            if (select)
            {
                SelectedSplitTab = tab;
            }
        }
        else
        {
            MainTabs.Add(tab);
            if (select)
            {
                SelectedMainTab = tab;
            }
        }
    }

    public void CloseTab(ITab tab)
    {
        tab.CloseRequested -= OnTabCloseRequested;

        if (tab is IFocusableTab focusableTab)
        {
            focusableTab.FocusRequested -= OnFocusRequested;
        }

        bool isMain = MainTabs.Contains(tab);
        ObservableCollection<ITab> targetList = isMain ? MainTabs : SplitTabs;

        targetList.Remove(tab);
        tabViewLocator.RemoveFromCache(tab);

        if (MainTabs.Count == 0 && SplitTabs.Count > 0)
        {
            List<ITab> itemsToMove = SplitTabs.ToList();
            SplitTabs.Clear();

            foreach (ITab item in itemsToMove)
            {
                MainTabs.Add(item);
            }
        }

        if (SelectedTab == tab)
        {
            ITab? nextTab = MainTabs.LastOrDefault() ?? SplitTabs.LastOrDefault();
            SelectedTab = nextTab;
        }

        if (tab is IFeatureTab featureTab)
        {
            features.OnTabClosed(featureTab);
        }

        TabClosed?.Invoke(tab);
    }

    public void SplitTab(ITab tab)
    {
        if (!MainTabs.Contains(tab))
        {
            return;
        }

        if (MainTabs.Count == 1)
        {
            notifs.Show(Notification
                .Create(TranslationKey.SplitViewOnlyTabErrorTitle)
                .WithMessage(TranslationKey.SplitViewOnlyTabErrorMessage)
                .WithType(NotificationType.Warning));

            return;
        }

        MainTabs.Remove(tab);
        SplitTabs.Add(tab);
        SelectedSplitTab = tab;
    }

    public void MergeTab(ITab tab)
    {
        if (!SplitTabs.Contains(tab))
        {
            return;
        }

        SplitTabs.Remove(tab);
        MainTabs.Add(tab);
        SelectedMainTab = tab;
    }

    public void OpenUniqueTab<T>(Func<T> factory, bool allowMultiple) where T : ITab
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
        if (sender is ITab tab)
        {
            CloseTab(tab);
        }
    }

    private void OnFocusRequested(object? sender, EventArgs e)
    {
        if (sender is ITab tab)
        {
            SelectedTab = tab;
        }
    }

    public event Action<ITab?>? SelectedTabChanged;
    public event Action<ITab>? TabClosed;
}