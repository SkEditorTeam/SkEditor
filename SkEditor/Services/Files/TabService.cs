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
            field = value;
            SelectedTabChanged?.Invoke(value);
        }
    }

    public event Action<Tab?>? SelectedTabChanged;

    public void OpenTab(Tab tab)
    {
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    public void CloseTab(Tab tab)
    {
        Tabs.Remove(tab);
        if (SelectedTab == tab)
            SelectedTab = Tabs.LastOrDefault();
    }
}