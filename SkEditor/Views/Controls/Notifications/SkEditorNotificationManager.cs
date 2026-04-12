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

namespace SkEditor.Views.Controls.Notifications;

[TemplatePart("PART_Items", typeof(Panel))]
[PseudoClasses(":topleft", ":topright", ":bottomleft", ":bottomright", ":topcenter", ":bottomcenter")]
public class SkEditorNotificationManager : TemplatedControl
{
    public static readonly StyledProperty<NotificationPosition> PositionProperty =
        WindowNotificationManager.PositionProperty.AddOwner<SkEditorNotificationManager>();

    public static readonly StyledProperty<int> MaxItemsProperty =
        WindowNotificationManager.MaxItemsProperty.AddOwner<SkEditorNotificationManager>();

    private readonly List<SkEditorNotificationCard> _items = [];
    private Panel? _itemsControl;

    static SkEditorNotificationManager()
    {
        HorizontalAlignmentProperty.OverrideDefaultValue<SkEditorNotificationManager>(HorizontalAlignment.Stretch);
        VerticalAlignmentProperty.OverrideDefaultValue<SkEditorNotificationManager>(VerticalAlignment.Stretch);
    }

    public SkEditorNotificationManager() => UpdatePseudoClasses(Position);

    public SkEditorNotificationManager(TopLevel host) : this() => InstallFromTopLevel(host);

    protected override Type StyleKeyOverride => typeof(WindowNotificationManager);

    public NotificationPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public int MaxItems
    {
        get => GetValue(MaxItemsProperty);
        set => SetValue(MaxItemsProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsControl = e.NameScope.Find<Panel>("PART_Items");
    }

    public void Show(string title, string message, NotificationType type, TimeSpan? expiration = null,
        Action? onClick = null)
    {
        Dispatcher.UIThread.VerifyAccess();

        SkEditorNotificationCard card = new()
        {
            Content = new Notification(title, message, type),
            NotificationType = type
        };

        card.NotificationClosed += (_, _) =>
        {
            _items.Remove(card);
            _itemsControl?.Children.Remove(card);
        };

        card.PointerPressed += (_, _) =>
        {
            onClick?.Invoke();
            card.Close();
        };

        _items.Add(card);
        _itemsControl?.Children.Add(card);

        if (_items.Count(i => !i.IsClosing) > MaxItems)
        {
            _items.First(i => !i.IsClosing).Close();
        }

        card.StartTimer(expiration ?? TimeSpan.FromSeconds(5));
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