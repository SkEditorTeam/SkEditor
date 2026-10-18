using System.Collections.Generic;
using R3;
using SkEditor.Layout.StatusBar.UI;

namespace SkEditor.Layout.StatusBar.Providers;

public interface IStatusBarProvider
{
    ReadOnlyReactiveProperty<bool> CanAttach { get; }

    IEnumerable<StatusBarItem> GetItems();
}