using System;
using System.Collections.Generic;
using System.ComponentModel;
using AvaloniaEdit.Document;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Tabs;
using SkEditor.ViewModels.Tabs.Capabilities;
using TabViewModelBase = SkEditor.ViewModels.Tabs.TabViewModelBase;

namespace SkEditor.Utilities.StatusBar;

public class EditorInfoStatusBarProvider(ITabService tabs, IWindowDialogService windowDialogs)
    : IStatusBarProvider
{
    private readonly StatusBarItem _cursorItem = new StatusBarItem()
        .WithAlignment(StatusBarItem.ItemAlignment.Right)
        .WithCommand(windowDialogs.ShowGoToLineDialogAsync);

    private TabViewModelBase? _currentTab;

    public bool CanAttach => true;

    public void Initialize()
    {
        tabs.SelectedTabChanged += OnTabChanged;
        OnTabChanged(tabs.SelectedTab);

        AttachStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? AttachStatusChanged;

    public IEnumerable<StatusBarItem> GetItems()
    {
        yield return _cursorItem;
    }

    private void OnTabChanged(TabViewModelBase? tab)
    {
        if (_currentTab != null)
        {
            _currentTab.PropertyChanged -= OnTabPropertyChanged;
        }

        _currentTab = tab;

        if (tab != null)
        {
            tab.PropertyChanged += OnTabPropertyChanged;
        }

        UpdateState();
    }

    private void UpdateState()
    {
        if (tabs.SelectedTab is ITextEditorTab editorTab)
        {
            _cursorItem.IsVisible = true;

            int offset = editorTab.CaretOffset;
            TextLocation location = editorTab.Document.GetLocation(offset);

            _cursorItem.Text = TranslatableString.FromRawString($"{location.Line}:{location.Column}");
        }
        else
        {
            _cursorItem.IsVisible = false;
        }
    }

    private void OnTabPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FileTabViewModel.CaretOffset))
        {
            UpdateState();
        }
    }
}