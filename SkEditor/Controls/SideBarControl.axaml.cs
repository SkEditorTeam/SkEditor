using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Controls;

public partial class SideBarControl : UserControl
{
    public static TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(250);

    private const int GapColumnIndex = 1;
    private const int ContentColumnIndex = 2;
    private const double GapWidth = 10.0;
    private const double ZeroWidth = 0.0;

    private readonly SolidColorBrush _buttonForeground = new(Color.Parse("#cccccc"));
    private readonly SolidColorBrush _activeButtonForeground = new(Color.Parse("#60cdff"));
    private readonly TimeSpan _toggleDebounceTime = TimeSpan.FromMilliseconds(250);

    private SidebarPanel? _currentPanel;
    private bool _isAnimating;
    private CancellationTokenSource? _animationCancellationSource;
    private DateTime _lastToggleTime = DateTime.MinValue;

    public SideBarControl()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow?.Splitter == null) return;

        mainWindow.Splitter.DragCompleted += Splitter_DragCompleted;

        if (_currentPanel != null) return;

        SetColumnWidths(mainWindow, ZeroWidth, ZeroWidth, ZeroWidth);
        mainWindow.SidebarContentBorder.Child = null;
    }

    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StopAnimation();
        var mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow?.Splitter != null)
        {
            mainWindow.Splitter.DragCompleted -= Splitter_DragCompleted;
        }
    }

    private void Splitter_DragCompleted(object? sender, Avalonia.Input.VectorEventArgs e)
    {
        var mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow == null || _currentPanel == null || _isAnimating)
            return;

        var currentWidth = mainWindow.CoreGrid.ColumnDefinitions[ContentColumnIndex].Width.Value;
        SkEditorAPI.Core.GetAppConfig().SidebarPanelSizes[_currentPanel.GetId()] = (int)currentWidth;
        mainWindow.CoreGrid.ColumnDefinitions[ContentColumnIndex].MinWidth = _currentPanel.DesiredWidth;
    }

    private void StopAnimation()
    {
        _animationCancellationSource?.Cancel();
        _animationCancellationSource?.Dispose();
        _animationCancellationSource = null;
    }

    private void UpdateSplitterVisibility()
    {
        var mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow?.Splitter == null) return;
        mainWindow.Splitter.IsEnabled = _currentPanel != null && !_isAnimating;
    }

    public void ReloadPanels()
    {
        var mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow == null) return;

        StopAnimation();
        _isAnimating = false;

        var panels = Registries.SidebarPanels;
        if (!panels.Any())
        {
            IsVisible = false;
            SetColumnWidths(mainWindow, ZeroWidth, ZeroWidth, ZeroWidth);
            mainWindow.SidebarContentBorder.Child = null;
            _currentPanel = null;
            UpdateSplitterVisibility();
            return;
        }

        IsVisible = true;
        Buttons.Children.Clear();
        mainWindow.SidebarContentBorder.Child = null;
        _currentPanel = null;

        SetColumnWidths(mainWindow, ZeroWidth, ZeroWidth, ZeroWidth);
        UpdateSplitterVisibility();

        foreach (var panel in panels)
            Buttons.Children.Add(CreatePanelButton(panel));
    }

    private Button CreatePanelButton(SidebarPanel panel)
    {
        Viewbox iconViewBox = CreateIconViewbox(panel.Icon, _buttonForeground);
        Viewbox activeIconViewBox = CreateIconViewbox(panel.IconActive, _activeButtonForeground);

        Button button = new()
        {
            Height = 48,
            Width = 48,
            Classes = { "barButton" },
            Content = iconViewBox,
            IsEnabled = !panel.IsDisabled,
            Tag = (iconViewBox, activeIconViewBox, panel)
        };

        button.Command = new RelayCommand(() => TogglePanel(panel, button));

        return button;
    }

    private void TogglePanel(SidebarPanel panel, Button button)
    {
        if (panel.IsDisabled) return;
        
        bool useAnimation = SkEditorAPI.Core.GetAppConfig().IsSidebarAnimationEnabled;

        if (useAnimation)
        {
            var now = DateTime.Now;
            if ((now - _lastToggleTime) < _toggleDebounceTime && _currentPanel != panel)
            {
                return;
            }

            _lastToggleTime = now;
        }

        StopAnimation();

        var mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow == null) return;

        var contentColumn = mainWindow.CoreGrid.ColumnDefinitions[ContentColumnIndex];
        var gapColumn = mainWindow.CoreGrid.ColumnDefinitions[GapColumnIndex];
        var sidebarContentBorder = mainWindow.SidebarContentBorder;

        if (_currentPanel == panel)
        {
            ClosePanelInternal(panel, button, contentColumn, gapColumn, sidebarContentBorder, useAnimation);
        }
        else
        {
            OpenPanelInternal(panel, button, contentColumn, gapColumn, sidebarContentBorder, useAnimation);
        }
    }

    private void OpenPanelInternal(SidebarPanel panel, Button button, ColumnDefinition contentColumn,
        ColumnDefinition gapColumn, Border sidebarContentBorder, bool animate)
    {
        var previousPanel = _currentPanel;
        var previousContent = sidebarContentBorder.Child;

        ResetButtonIcons();

        if (previousPanel != null)
        {
            previousPanel.OnClose();
            if (previousContent != null) ResetControlAlignment(previousContent);
            sidebarContentBorder.Child = null;
        }

        _currentPanel = panel;
        SetButtonActive(button, true);
        var panelContent = _currentPanel.Content;
        var targetContentWidth = GetPanelWidth(_currentPanel);
        contentColumn.MinWidth = 0;

        if (panelContent != null)
        {
            ConfigurePanelContentForAnimation(panelContent, targetContentWidth);
            sidebarContentBorder.Child = panelContent;
        }

        _currentPanel.OnOpen();

        if (animate)
        {
            _isAnimating = true;
            UpdateSplitterVisibility();
            AnimateColumnsAsync(contentColumn, targetContentWidth, gapColumn, GapWidth)
                .ContinueWith(task => Dispatcher.UIThread.Post(() =>
                        FinalizeOpen(panel, panelContent, contentColumn, sidebarContentBorder,
                            task.IsCompletedSuccessfully)),
                    TaskScheduler.FromCurrentSynchronizationContext());
        }
        else
        {
            contentColumn.Width = new GridLength(targetContentWidth);
            gapColumn.Width = new GridLength(GapWidth);
            FinalizeOpen(panel, panelContent, contentColumn, sidebarContentBorder, true);
        }
    }

    private void ClosePanelInternal(SidebarPanel panelToClose, Button button, ColumnDefinition contentColumn,
        ColumnDefinition gapColumn, Border sidebarContentBorder, bool animate)
    {
        _currentPanel = null;
        var contentToClose = sidebarContentBorder.Child;

        SetButtonActive(button, false);

        if (contentToClose != null)
        {
            contentToClose.Width = contentColumn.Width.Value;
            contentToClose.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        }

        contentColumn.MinWidth = 0;

        if (animate)
        {
            _isAnimating = true;
            UpdateSplitterVisibility();
            AnimateColumnsAsync(contentColumn, ZeroWidth, gapColumn, ZeroWidth)
                .ContinueWith(task => Dispatcher.UIThread.Post(() =>
                        FinalizeClose(panelToClose, contentToClose, sidebarContentBorder,
                            task.IsCompletedSuccessfully)),
                    TaskScheduler.FromCurrentSynchronizationContext());
        }
        else
        {
            contentColumn.Width = new GridLength(ZeroWidth);
            gapColumn.Width = new GridLength(ZeroWidth);
            FinalizeClose(panelToClose, contentToClose, sidebarContentBorder, true);
        }
    }

    private void FinalizeOpen(SidebarPanel panel, Control? panelContent, ColumnDefinition contentColumn, Border sidebarContentBorder, bool success)
    {
        _isAnimating = false;

        if (!success)
        {
            UpdateSplitterVisibility();
            return;
        }

        if (_currentPanel == panel)
        {
            contentColumn.MinWidth = _currentPanel.DesiredWidth;
            if (panelContent != null)
            {
                ResetControlAlignment(panelContent);
            }
        }
        else
        {
            if (_currentPanel == null)
            {
                SetColumnWidths(SkEditorAPI.Windows.GetMainWindow()!, ZeroWidth, ZeroWidth, ZeroWidth);
            }

            if (panelContent != null && sidebarContentBorder.Child != panelContent)
            {
                ResetControlAlignment(panelContent);
            }
        }

        UpdateSplitterVisibility();
    }

    private void FinalizeClose(SidebarPanel panelToClose, Control? contentToClose, Border sidebarContentBorder,
        bool success)
    {
        _isAnimating = false;

        panelToClose.OnClose();

        if (success && sidebarContentBorder.Child == contentToClose)
        {
            sidebarContentBorder.Child = null;
        }

        if (contentToClose != null)
        {
            ResetControlAlignment(contentToClose);
        }

        UpdateSplitterVisibility();
    }

    private static void SetColumnWidths(MainWindow mainWindow, double gapWidth, double contentWidth, double minWidth)
    {
        if (mainWindow == null) return;
        mainWindow.CoreGrid.ColumnDefinitions[GapColumnIndex].Width = new GridLength(gapWidth);
        mainWindow.CoreGrid.ColumnDefinitions[ContentColumnIndex].Width = new GridLength(contentWidth);
        mainWindow.CoreGrid.ColumnDefinitions[ContentColumnIndex].MinWidth = minWidth;
    }

    private static void ConfigurePanelContentForAnimation(Control panelContent, double width)
    {
        panelContent.Width = width;
        panelContent.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        panelContent.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
    }

    private static void ResetControlAlignment(Control control)
    {
        control.Width = double.NaN;
        control.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
    }

    private static double GetPanelWidth(SidebarPanel panel)
    {
        return SkEditorAPI.Core.GetAppConfig().SidebarPanelSizes.GetValueOrDefault(panel.GetId(), panel.DesiredWidth);
    }

    private static void SetButtonActive(Button button, bool isActive)
    {
        if (button.Tag is not (Viewbox icon, Viewbox activeIcon, SidebarPanel _)) return;
        button.Content = isActive ? activeIcon : icon;
    }

    private void ResetButtonIcons()
    {
        foreach (var child in Buttons.Children)
        {
            if (child is Button btn)
            {
                SetButtonActive(btn, false);
            }
        }
    }

    private async Task<bool> AnimateColumnsAsync(ColumnDefinition contentColumn, double targetContentWidth,
        ColumnDefinition gapColumn, double targetGapWidth)
    {
        StopAnimation();
        _animationCancellationSource = new CancellationTokenSource();
        var token = _animationCancellationSource.Token;

        var startContentWidth = contentColumn.Width.IsAbsolute ? contentColumn.Width.Value : 0;
        var startGapWidth = gapColumn.Width.IsAbsolute ? gapColumn.Width.Value : 0;

        contentColumn.Width = new GridLength(startContentWidth, GridUnitType.Pixel);
        gapColumn.Width = new GridLength(startGapWidth, GridUnitType.Pixel);

        var duration = TransitionDuration.TotalMilliseconds;
        if (duration <= 0)
        {
            contentColumn.Width = new GridLength(targetContentWidth, GridUnitType.Pixel);
            gapColumn.Width = new GridLength(targetGapWidth, GridUnitType.Pixel);
            return true;
        }

        var startTime = DateTime.UtcNow;
        var tcs = new TaskCompletionSource<bool>();

        void AnimationTick(TimeSpan time)
        {
            if (token.IsCancellationRequested)
            {
                tcs.TrySetCanceled(token);
                return;
            }

            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
            double progress = Math.Clamp(elapsed / duration, 0.0, 1.0);
            double easedProgress = 1 - Math.Pow(1 - progress, 3);

            double currentContentWidth = startContentWidth + (targetContentWidth - startContentWidth) * easedProgress;
            double currentGapWidth = startGapWidth + (targetGapWidth - startGapWidth) * easedProgress;

            contentColumn.Width = new GridLength(currentContentWidth, GridUnitType.Pixel);
            gapColumn.Width = new GridLength(currentGapWidth, GridUnitType.Pixel);

            if (progress < 1.0)
            {
                Dispatcher.UIThread.InvokeAsync(() => TopLevel.GetTopLevel(this)?.RequestAnimationFrame(AnimationTick),
                    DispatcherPriority.Render);
            }
            else
            {
                contentColumn.Width = new GridLength(targetContentWidth, GridUnitType.Pixel);
                gapColumn.Width = new GridLength(targetGapWidth, GridUnitType.Pixel);
                tcs.TrySetResult(true);
            }
        }

        Dispatcher.UIThread.Post(() => TopLevel.GetTopLevel(this)?.RequestAnimationFrame(AnimationTick),
            DispatcherPriority.Background);

        try
        {
            return await tcs.Task;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    private static Viewbox CreateIconViewbox(IconSource icon, SolidColorBrush foreground)
    {
        return new Viewbox
        {
            Child = new IconSourceElement
            {
                IconSource = icon,
                Foreground = foreground,
            },
            Width = 22,
            Height = 22
        };
    }
}