using System;
using System.Collections.ObjectModel;
using System.Linq;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Tabs;

namespace SkEditor.Services.Files;

public class TabService : ITabService
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

    public void OpenTab(Tab tab)
    {
        tab.CloseRequested += OnTabCloseRequested;
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    public void CloseTab(Tab tab)
    {
        tab.CloseRequested -= OnTabCloseRequested;
        Tabs.Remove(tab);
        if (SelectedTab == tab)
            SelectedTab = Tabs.LastOrDefault();
    }

    private void OnTabCloseRequested(object? sender, EventArgs e)
    {
        if (sender is Tab tab) CloseTab(tab);
    }
}