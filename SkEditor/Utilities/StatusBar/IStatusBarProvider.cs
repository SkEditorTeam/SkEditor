using System.Collections.Generic;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Utilities.StatusBar;

public interface IStatusBarProvider
{
    IEnumerable<StatusBarItem> GetItems();
}