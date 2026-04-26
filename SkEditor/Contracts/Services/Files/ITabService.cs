using System;
using System.Collections.ObjectModel;
using SkEditor.Models.Tabs;

namespace SkEditor.Contracts.Services.Files;

public interface ITabService
{
    ObservableCollection<Tab> Tabs { get; }
    Tab? SelectedTab { get; set; }
    event Action<Tab?>? SelectedTabChanged;
    void OpenTab(Tab tab);
    void CloseTab(Tab tab);

    void OpenUniqueTab<T>(Func<T> factory) where T : Tab;
}