using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications.Converters;

namespace SkEditor.Core.Notifications.UI;

[TemplatePart("PART_Items", typeof(Panel))]
[PseudoClasses(":topleft", ":topright", ":bottomleft", ":bottomright", ":topcenter", ":bottomcenter")]
public class SkEditorNotificationManager : TemplatedControl
{
    public static readonly StyledProperty<NotificationPosition> PositionProperty =
        WindowNotificationManager.PositionProperty.AddOwner<SkEditorNotificationManager>();

    public static readonly StyledProperty<int> MaxItemsProperty =
        WindowNotificationManager.MaxItemsProperty.AddOwner<SkEditorNotificationManager>();
    
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(4);

    private readonly List<SkEditorNotificationCard> _items = [];
    private Panel? _itemsControl;

    public int MaxItems
    {
        get => GetValue(MaxItemsProperty);
        set => SetValue(MaxItemsProperty, value);
    }

    public NotificationPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(WindowNotificationManager);

    static SkEditorNotificationManager()
    {
        HorizontalAlignmentProperty.OverrideDefaultValue<SkEditorNotificationManager>(HorizontalAlignment.Stretch);
        VerticalAlignmentProperty.OverrideDefaultValue<SkEditorNotificationManager>(VerticalAlignment.Stretch);
    }

    public SkEditorNotificationManager() => UpdatePseudoClasses(Position);

    public SkEditorNotificationManager(TopLevel host) : this() => InstallFromTopLevel(host);

    public void Show(Notification notification, Action<Notification>? onClosed = null)
    {
        Dispatcher.UIThread.VerifyAccess();

        var type = NotificationTypeConverter.ToAvaloniaType(notification.Type);

        SkEditorNotificationCard card = new()
        {
            Content = new Avalonia.Controls.Notifications.Notification(
                notification.Title.Value,
                notification.Message?.Value ?? string.Empty,
                type, notification.Duration ?? DefaultDuration),
            NotificationType = type
        };

        card.NotificationClosed += (_, _) =>
        {
            _items.Remove(card);
            _itemsControl?.Children.Remove(card);
        
            onClosed?.Invoke(notification);
        };

        card.PointerPressed += async (_, e) =>
        {
            if (card.IsClosing || !e.GetCurrentPoint(card).Properties.IsLeftButtonPressed)
            {
                return;
            }

            e.Handled = true;
            card.Close();

            if (notification.OnClick == null)
            {
                return;
            }

            try
            {
                await notification.OnClick();
            }
            catch (Exception ex)
            {
                App.Services.GetRequiredService<ILogService>().Error(ex, "Notification action failed");
            }
        };

        _items.Add(card);
        _itemsControl?.Children.Add(card);

        if (_items.Count(i => !i.IsClosing) > MaxItems)
        {
            _items.First(i => !i.IsClosing).Close();
        }

        card.StartTimer(notification.Duration ?? TimeSpan.FromSeconds(5));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsControl = e.NameScope.Find<Panel>("PART_Items");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PositionProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<NotificationPosition>());
        }
    }

    private void InstallFromTopLevel(TopLevel topLevel)
    {
        topLevel.TemplateApplied += TopLevelOnTemplateApplied;
        AdornerLayer? adorner = AdornerLayer.GetAdornerLayer(topLevel);
        if (adorner is null)
        {
            return;
        }

        adorner.Children.Add(this);
        AdornerLayer.SetAdornedElement(this, adorner);
    }

    private void TopLevelOnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (Parent is AdornerLayer adornerLayer)
        {
            adornerLayer.Children.Remove(this);
            AdornerLayer.SetAdornedElement(this, null);
        }

        TopLevel topLevel = (TopLevel)sender!;
        topLevel.TemplateApplied -= TopLevelOnTemplateApplied;
        InstallFromTopLevel(topLevel);
    }

    private void UpdatePseudoClasses(NotificationPosition position)
    {
        PseudoClasses.Set(":topleft", position == NotificationPosition.TopLeft);
        PseudoClasses.Set(":topright", position == NotificationPosition.TopRight);
        PseudoClasses.Set(":bottomleft", position == NotificationPosition.BottomLeft);
        PseudoClasses.Set(":bottomright", position == NotificationPosition.BottomRight);
        PseudoClasses.Set(":topcenter", position == NotificationPosition.TopCenter);
        PseudoClasses.Set(":bottomcenter", position == NotificationPosition.BottomCenter);
    }
}