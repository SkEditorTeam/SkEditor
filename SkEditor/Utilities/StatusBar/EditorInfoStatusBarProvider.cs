using System.Collections.Generic;
using System.ComponentModel;
using AvaloniaEdit.Document;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Utilities.StatusBar;

public class EditorInfoStatusBarProvider : IStatusBarProvider
{
    private readonly ITabService _tabService;
    private readonly StatusBarItem _cursorItem;

    private Tab? _currentTab;

    public EditorInfoStatusBarProvider(ITabService tabService, IWindowDialogService windowDialogs)
    {
        _tabService = tabService;

        _cursorItem = new StatusBarItem()
            .WithAlignment(StatusBarItem.ItemAlignment.Right)
            .WithCommand(windowDialogs.ShowGoToLineDialogAsync);

        _tabService.SelectedTabChanged += OnTabChanged;

        UpdateState();
    }

    private void OnTabChanged(Tab? tab)
    {
        _currentTab?.PropertyChanged -= OnTabPropertyChanged;
        _currentTab = tab;

        tab?.PropertyChanged += OnTabPropertyChanged;

        UpdateState();
    }

    public IEnumerable<StatusBarItem> GetItems()
    {
        yield return _cursorItem;
    }

    private void UpdateState()
    {
        if (_tabService.SelectedTab is FileTab fileTab)
        {
            _cursorItem.IsVisible = true;

            int offset = fileTab.CaretOffset;
            TextLocation location = fileTab.Document.GetLocation(offset);

            _cursorItem.Text = TranslatableString.FromRawString($"{location.Line}:{location.Column}");
        }
        else
        {
            _cursorItem.IsVisible = false;
        }
    }

    private void OnTabPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FileTab.CaretOffset))
        {
            UpdateState();
        }
    }
}