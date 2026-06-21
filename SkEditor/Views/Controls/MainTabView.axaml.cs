using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using HotAvalonia;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.DataTemplates;
using SkEditor.ViewModels.Tabs;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Views.Controls;

public partial class MainTabView : UserControl
{
    private GridLength _savedSplitWidth = new(1, GridUnitType.Star);

    public MainTabView()
    {
        InitializeComponent();
        InitializeComponentState();
    }

    [AvaloniaHotReload]
    private void InitializeComponentState()
    {
        TabViewLocator locator = App.Services.GetRequiredService<TabViewLocator>();
        TabView.DataTemplates.Add(locator);
        SplitTabView.DataTemplates.Add(locator);

        Splitter.DragCompleted += (_, _) =>
        {
            if (TabGrid.ColumnDefinitions.Count < 3)
            {
                return;
            }

            _savedSplitWidth = TabGrid.ColumnDefinitions[2].Width;
        };


        TabView.TabDragCompleted += OnTabDragDrop;
        SplitTabView.TabDragCompleted += OnTabDragDrop;
        TabView.TabStripDragOver += OnTabStripDragOver;
        SplitTabView.TabStripDragOver += OnTabStripDragOver;

        if (DataContext is MainWindowViewModel mainVm)
        {
            UpdateSplitState(mainVm.IsSplitActive);
        }
    }

    private static void OnTabStripDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Move;
        e.Handled = true;
    }
    
    private void OnTabDragDrop(FATabView source, FATabViewTabDragCompletedEventArgs e)
    {
        if (e.Tab.DataContext is not TabViewModelBase tabVm)
        {
            return;
        }

        if (DataContext is not MainWindowViewModel mainVm)
        {
            return;
        }

        if (!mainVm.IsSplitActive)
        {
            return;
        }

        if (e.DropResult != DragDropEffects.None)
        {
            return;
        }

        Dispatcher.InvokeAsync(() =>
        {
            if (source.Name == nameof(TabView))
            {
                mainVm.SplitTab(tabVm);
            }
            else
            {
                mainVm.MergeTab(tabVm);
            }
        });
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        vm.PropertyChanged += OnViewModelPropertyChanged;
        UpdateSplitState(vm.IsSplitActive);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.IsSplitActive) && DataContext is MainWindowViewModel vm)
        {
            UpdateSplitState(vm.IsSplitActive);
        }
    }

    private void UpdateSplitState(bool isActive)
    {
        if (TabGrid.ColumnDefinitions.Count < 3)
        {
            return;
        }

        ColumnDefinition? splitterCol = TabGrid.ColumnDefinitions[1];
        ColumnDefinition? splitCol = TabGrid.ColumnDefinitions[2];

        if (splitCol == null || splitterCol == null)
        {
            return;
        }

        if (isActive)
        {
            splitCol.Width = _savedSplitWidth;
            splitCol.MinWidth = 300;
            splitterCol.Width = GridLength.Auto;
        }
        else
        {
            if (splitCol.Width.Value > 0)
            {
                _savedSplitWidth = splitCol.Width;
            }

            splitCol.MinWidth = 0;
            splitCol.Width = new GridLength(0);
            splitterCol.Width = new GridLength(0);
        }
    }
}