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
using System.Threading.Tasks;

namespace SkEditor.Controls;

public partial class SideBarControl : UserControl
{
    private SidebarPanel? _currentPanel;
    public static TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(250);

    private readonly SolidColorBrush _buttonForeground = new(Color.Parse("#cccccc"));
    private readonly SolidColorBrush _activeButtonForeground = new(Color.Parse("#60cdff"));

    private bool _isAnimating;
    private TaskCompletionSource<bool>? _animationCompletionSource;

    private const int GapColumnIndex = 1;
    private const int ContentColumnIndex = 2;
    private const double GapWidth = 10.0;
    private const double ZeroWidth = 0.0;

    public SideBarControl()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        Loaded += (_, _) => OnLoaded();
        Unloaded += (_, _) => StopAnimation();
    }

    private void OnLoaded()
    {
        var mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow?.Splitter == null) return;

        mainWindow.Splitter.DragCompleted += (_, _) =>
        {
            if (_currentPanel == null || _isAnimating)
                return;

            var currentWidth = mainWindow.CoreGrid.ColumnDefinitions[ContentColumnIndex].Width.Value;
            SkEditorAPI.Core.GetAppConfig().SidebarPanelSizes[_currentPanel.GetId()] = (int)currentWidth;

            mainWindow.CoreGrid.ColumnDefinitions[ContentColumnIndex].MinWidth = _currentPanel.DesiredWidth;
        };

        if (_currentPanel != null) return;

        SetColumnWidths(mainWindow, ZeroWidth, ZeroWidth, 0);
        mainWindow.SidebarContentBorder.Child = null;
    }

    private void StopAnimation()
    {
        _animationCompletionSource?.TrySetResult(false);
        _animationCompletionSource = null;
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
            SetColumnWidths(mainWindow, ZeroWidth, ZeroWidth, 0);
            return;
        }

        IsVisible = true;
        Buttons.Children.Clear();
        mainWindow.SidebarContentBorder.Child = null;
        _currentPanel = null;

        SetColumnWidths(mainWindow, ZeroWidth, ZeroWidth, 0);
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
        if (panel.IsDisabled)
            return;

        if (_isAnimating)
        {
            StopAnimation();
        }

        _isAnimating = true;
        UpdateSplitterVisibility();

        var mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow == null)
        {
            _isAnimating = false;
            UpdateSplitterVisibility();
            return;
        }

        var contentColumn = mainWindow.CoreGrid.ColumnDefinitions[ContentColumnIndex];
        var gapColumn = mainWindow.CoreGrid.ColumnDefinitions[GapColumnIndex];
        var sidebarContentBorder = mainWindow.SidebarContentBorder;

        if (_currentPanel == panel)
        {
            AnimateClose(contentColumn, gapColumn, sidebarContentBorder, button);
        }
        else
        {
            AnimateOpen(panel, button, contentColumn, gapColumn, sidebarContentBorder);
        }
    }

    private void AnimateOpen(SidebarPanel panel, Button button, ColumnDefinition contentColumn, ColumnDefinition gapColumn, Border sidebarContentBorder)
    {
        ResetButtonIcons();

        var previousPanel = _currentPanel;
        var previousContent = sidebarContentBorder.Child;

        if (previousPanel != null)
        {
            ClosePanel(previousPanel, previousContent, contentColumn);
            sidebarContentBorder.Child = null;
        }

        SetButtonActive(button, true);

        _currentPanel = panel;
        var panelContent = _currentPanel.Content;
        var targetContentWidth = GetPanelWidth(_currentPanel);

        if (panelContent != null)
        {
            ConfigurePanelContent(panelContent, targetContentWidth, sidebarContentBorder);
        }

        _currentPanel.OnOpen();

        AnimateColumnsAsync(contentColumn, targetContentWidth, gapColumn, GapWidth).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                FinalizePanelAnimation(panel, panelContent, contentColumn, gapColumn, sidebarContentBorder);
            });
        });
    }

    private void AnimateClose(ColumnDefinition contentColumn, ColumnDefinition gapColumn, Border sidebarContentBorder, Button button)
    {
        var panelToClose = _currentPanel;
        var contentToClose = sidebarContentBorder.Child;
        _currentPanel = null;

        SetButtonActive(button, false);

        if (contentToClose != null)
        {
            double currentWidth = contentColumn.Width.Value;
            contentToClose.Width = currentWidth;
            contentToClose.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        }

        contentColumn.MinWidth = 0;

        AnimateColumnsAsync(contentColumn, ZeroWidth, gapColumn, ZeroWidth).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                panelToClose?.OnClose();
                sidebarContentBorder.Child = null;

                if (contentToClose != null)
                {
                    ResetControlAlignment(contentToClose);
                }

                _isAnimating = false;
                UpdateSplitterVisibility();
            });
        });
    }

    #region Helper Methods

    private static void SetColumnWidths(MainWindow mainWindow, double gapWidth, double contentWidth, double minWidth)
    {
        mainWindow.CoreGrid.ColumnDefinitions[GapColumnIndex].Width = new GridLength(gapWidth);
        mainWindow.CoreGrid.ColumnDefinitions[ContentColumnIndex].Width = new GridLength(contentWidth);
        mainWindow.CoreGrid.ColumnDefinitions[ContentColumnIndex].MinWidth = minWidth;
    }

    private static void ClosePanel(SidebarPanel panel, Control? content, ColumnDefinition contentColumn)
    {
        panel.OnClose();
        contentColumn.MinWidth = 0;

        if (content != null)
        {
            ResetControlAlignment(content);
        }
    }

    private static void ConfigurePanelContent(Control panelContent, double width, Border sidebarContentBorder)
    {
        panelContent.Width = width;
        panelContent.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        panelContent.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        sidebarContentBorder.Child = panelContent;
    }

    private void FinalizePanelAnimation(SidebarPanel panel, Control? panelContent, ColumnDefinition contentColumn,
        ColumnDefinition gapColumn, Border sidebarContentBorder)
    {
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
            contentColumn.MinWidth = 0;
            if (panelContent != null && sidebarContentBorder.Child != panelContent)
            {
                ResetControlAlignment(panelContent);
            }
            gapColumn.Width = new GridLength(0);
        }

        _isAnimating = false;
        UpdateSplitterVisibility();
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
        if (button.Tag is not ValueTuple<Viewbox, Viewbox, SidebarPanel> buttonData) return;

        button.Content = isActive ? buttonData.Item2 : buttonData.Item1;
    }

    private void ResetButtonIcons()
    {
        foreach (var child in Buttons.Children)
        {
            if (child is Button { Tag: ValueTuple<Viewbox, Viewbox, SidebarPanel> } btn)
            {
                SetButtonActive(btn, false);
            }
        }
    }

    private Task<bool> AnimateColumnsAsync(ColumnDefinition contentColumn, double targetContentWidth, ColumnDefinition gapColumn, double targetGapWidth)
    {
        StopAnimation();
        _animationCompletionSource = new TaskCompletionSource<bool>();

        var startContentWidth = contentColumn.Width.Value;
        var startGapWidth = gapColumn.Width.Value;

        var duration = TransitionDuration.TotalMilliseconds;
        var startTime = DateTime.UtcNow;

        if (contentColumn.Width.GridUnitType != GridUnitType.Pixel)
            contentColumn.Width = new GridLength(startContentWidth, GridUnitType.Pixel);
        if (gapColumn.Width.GridUnitType != GridUnitType.Pixel)
            gapColumn.Width = new GridLength(startGapWidth, GridUnitType.Pixel);

        void AnimateFrame(TimeSpan _)
        {
            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
            double progress = duration <= 0 ? 1.0 : Math.Clamp(elapsed / duration, 0.0, 1.0);
            double easedProgress = 1 - Math.Pow(1 - progress, 3);

            double currentContentWidth = startContentWidth + (targetContentWidth - startContentWidth) * easedProgress;
            double currentGapWidth = startGapWidth + (targetGapWidth - startGapWidth) * easedProgress;

            contentColumn.Width = new GridLength(currentContentWidth, GridUnitType.Pixel);
            gapColumn.Width = new GridLength(currentGapWidth, GridUnitType.Pixel);

            if (progress < 1.0)
            {
                TopLevel.GetTopLevel(this)?.RequestAnimationFrame(AnimateFrame);
            }
            else
            {
                contentColumn.Width = new GridLength(targetContentWidth, GridUnitType.Pixel);
                gapColumn.Width = new GridLength(targetGapWidth, GridUnitType.Pixel);
                _animationCompletionSource?.TrySetResult(true);
            }
        }

        TopLevel.GetTopLevel(this)?.RequestAnimationFrame(AnimateFrame);
        return _animationCompletionSource.Task;
    }


    private static Viewbox CreateIconViewbox(IconSource icon, SolidColorBrush foreground)
    {
        var iconElement = new IconSourceElement()
        {
            IconSource = icon,
            Foreground = foreground,
        };

        return new Viewbox
        {
            Child = iconElement,
            Width = 26,
            Height = 26
        };
    }
    #endregion
}