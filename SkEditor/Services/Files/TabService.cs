using System;
using System.Collections.ObjectModel;
using System.Linq;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Models.Tabs;

namespace SkEditor.Services.Files;

public class TabService(IFileFeatureService features) : ITabService
{
    public ObservableCollection<Tab> Tabs { get; } = [];

    public Tab? SelectedTab
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            field = value;
            SelectedTabChanged?.Invoke(value);
        }
    }

    public event Action<Tab?>? SelectedTabChanged;
    public event Action<Tab>? TabClosed;

    public void SwitchTab(int index)
    {
        if (index < 0 || index >= Tabs.Count) return;
        SelectedTab = Tabs[index];
    }

    public void OpenTab(Tab tab)
    {
        tab.CloseRequested += OnTabCloseRequested;

        if (tab is FileTab fileTab)
        {
            features.OnTabOpened(fileTab);
        }

        Tabs.Add(tab);
        SelectedTab = tab;
    }

    public void CloseTab(Tab tab)
    {
        tab.CloseRequested -= OnTabCloseRequested;
        Tabs.Remove(tab);

        if (SelectedTab == tab)
            SelectedTab = Tabs.LastOrDefault();

        if (tab is FileTab fileTab)
        {
            features.OnTabClosed(fileTab);
        }

        TabClosed?.Invoke(tab);
    }

    public void OpenUniqueTab<T>(Func<T> factory, bool allowMultiple) where T : Tab
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
        if (sender is Tab tab) CloseTab(tab);
    }
}