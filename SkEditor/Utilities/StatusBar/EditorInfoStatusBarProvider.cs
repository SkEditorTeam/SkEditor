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

public class EditorInfoStatusBarProvider : IStatusBarProvider
{
    private readonly StatusBarItem _cursorItem;
    private readonly ITabService _tabService;

    private TabViewModelBase? _currentTab;

    public EditorInfoStatusBarProvider(ITabService tabService, IWindowDialogService windowDialogs)
    {
        _tabService = tabService;

        _cursorItem = new StatusBarItem()
            .WithAlignment(StatusBarItem.ItemAlignment.Right)
            .WithCommand(windowDialogs.ShowGoToLineDialogAsync);

        _tabService.SelectedTabChanged += OnTabChanged;

        UpdateState();
    }

    public IEnumerable<StatusBarItem> GetItems()
    {
        yield return _cursorItem;
    }

    private void OnTabChanged(TabViewModelBase? tab)
    {
        _currentTab?.PropertyChanged -= OnTabPropertyChanged;
        _currentTab = tab;

        tab?.PropertyChanged += OnTabPropertyChanged;

        UpdateState();
    }

    private void UpdateState()
    {
        if (_tabService.SelectedTab is ITextEditorTab editorTab)
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