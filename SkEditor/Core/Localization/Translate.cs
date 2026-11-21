using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace SkEditor.Core.Localization;

public sealed class Translate : AvaloniaObject
{
    public static readonly AttachedProperty<TranslationKey?> TextKeyProperty =
        AvaloniaProperty.RegisterAttached<Translate, TextBlock, TranslationKey?>("TextKey");

    private static readonly AttachedProperty<IDisposable?> SubscriptionProperty =
        AvaloniaProperty.RegisterAttached<Translate, TextBlock, IDisposable?>("Subscription");

    private static readonly AttachedProperty<bool> IsHookedProperty =
        AvaloniaProperty.RegisterAttached<Translate, TextBlock, bool>("IsHooked");

    static Translate() => TextKeyProperty.Changed.AddClassHandler<TextBlock>(OnTextKeyChanged);

    public static TranslationKey? GetTextKey(TextBlock element)
    {
        return element.GetValue(TextKeyProperty);
    }

    public static void SetTextKey(TextBlock element, TranslationKey? value)
    {
        element.SetValue(TextKeyProperty, value);
    }

    private static void OnTextKeyChanged(TextBlock tb, AvaloniaPropertyChangedEventArgs args)
    {
        if (!tb.GetValue(IsHookedProperty))
        {
            tb.SetValue(IsHookedProperty, true);
            tb.AttachedToLogicalTree += OnAttached;
            tb.DetachedFromLogicalTree += OnDetached;
        }

        UpdateBinding(tb);
    }

    private static void OnAttached(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        if (sender is TextBlock tb)
        {
            UpdateBinding(tb);
        }
    }

    private static void OnDetached(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        if (sender is not TextBlock tb)
        {
            return;
        }

        tb.GetValue(SubscriptionProperty)?.Dispose();
        tb.SetValue(SubscriptionProperty, null);
    }

    private static void UpdateBinding(TextBlock tb)
    {
        tb.GetValue(SubscriptionProperty)?.Dispose();
        tb.SetValue(SubscriptionProperty, null);

        if (GetTextKey(tb) is not { } key || key == TranslationKey.None)
        {
            tb.ClearValue(TextBlock.TextProperty);
            return;
        }

        IObservable<object?> observable = tb.GetResourceObservable(key.ToString());
        IDisposable subscription = tb.Bind((AvaloniaProperty)TextBlock.TextProperty, observable);
        tb.SetValue(SubscriptionProperty, subscription);
    }
}