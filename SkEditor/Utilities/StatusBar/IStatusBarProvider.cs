using System;
using System.Collections.Generic;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Utilities.StatusBar;

public interface IStatusBarProvider
{
    bool CanAttach { get; }

    void Initialize();

    event EventHandler? AttachStatusChanged;
    
    IEnumerable<StatusBarItem> GetItems();
}