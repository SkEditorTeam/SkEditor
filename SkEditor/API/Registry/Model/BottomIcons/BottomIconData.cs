using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.InternalAPI;

namespace SkEditor.API;

/// <summary>
/// Represent an icon in the bottom bar of SkEditor's window.
/// </summary>
public class BottomIconData : IBottomIconElement {
    
    public IconSource IconSource { get; set; }
    public int Order { get; }
    public string? Text { get; set; }
    public EventHandler<BottomIconElementClickedEventArgs>? Clicked { get; set; }
    public string Id { get; set; }
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the BottomIconData class.
    /// </summary>
    /// <param name="iconSource">The source of the icon.</param>
    /// <param name="id">The unique identifier of the icon, mainly used in groups to identify the icon.</param>
    /// <param name="text">The text associated with the icon. Can be null.</param>
    /// <param name="clicked">The event handler for the click event. Can be null.</param>
    /// <param name="order">The order of the icon in the bottom bar, or in the group if part of a group.</param>
    public BottomIconData(IconSource iconSource, string id, string? text, EventHandler<BottomIconElementClickedEventArgs>? clicked = null,
        int order = 0, bool isEnabled = true)
    {
        IconSource = iconSource;
        Id = id;
        Order = order;
        Text = text;
        Clicked = clicked;
        IsEnabled = isEnabled;
    }
    
    private bool _initialized;
    private Button? _attachedButton; // will be null if it's a group
    
    private TextBlock _attachedTextBlock = null!;
    private IconSourceElement _attachedIconElement = null!;

    public void Setup(Button? button, TextBlock textBlock, IconSourceElement iconElement)
    {
        _initialized = true;
        _attachedButton = button;
        _attachedTextBlock = textBlock;
        _attachedIconElement = iconElement;
        
        if (_attachedButton != null) 
            _attachedButton.Click += (sender, _) => AddonLoader.HandleAddonMethod(() => Clicked?.Invoke(sender, new BottomIconElementClickedEventArgs(this)));
        _attachedTextBlock.Text = Text;
        _attachedTextBlock.IsVisible = Text != null;
        _attachedIconElement.IconSource = IconSource;
    }
    
    /// <summary>
    /// Updates the text of the icon.
    /// </summary>
    /// <param name="text">The new text to set.</param>
    /// <exception cref="InvalidOperationException">This BottomIconData has not been initialized yet.</exception>
    public void UpdateText(string? text)
    {
        if (!_initialized)
            throw new InvalidOperationException("This BottomIconData has not been initialized yet.");
        
        _attachedTextBlock.Text = text;
        _attachedTextBlock.IsVisible = text != null;
    }
    
    /// <summary>
    /// Updates the icon of the BottomIconData instance.
    /// </summary>
    /// <param name="icon">The new icon to set.</param>
    /// <exception cref="InvalidOperationException">This BottomIconData has not been initialized yet.</exception>
    public void UpdateIcon(IconSource icon)
    {
        if (!_initialized)
            throw new InvalidOperationException("This BottomIconData has not been initialized yet.");
        
        _attachedIconElement.IconSource = icon;
    }
    
    public void UpdateEnabled(bool enabled)
    {
        if (!_initialized)
            throw new InvalidOperationException("This BottomIconData has not been initialized yet.");
        
        _attachedButton.IsEnabled = enabled;
    }
    
    public Button? GetButton() => _attachedButton;
    public TextBlock GetTextBlock() => _attachedTextBlock;
    public IconSourceElement GetIconElement() => _attachedIconElement;
    public bool IsInitialized() => _initialized;

}