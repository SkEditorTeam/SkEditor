using System.Collections.Generic;
using R3;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Utilities.StatusBar;

public interface IStatusBarProvider
{
    ReadOnlyReactiveProperty<bool> CanAttach { get; }

    IEnumerable<StatusBarItem> GetItems();
}