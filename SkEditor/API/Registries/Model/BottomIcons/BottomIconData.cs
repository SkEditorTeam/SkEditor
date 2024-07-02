using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.InternalAPI;
using System;

namespace SkEditor.API;

/// <summary>
/// Represent an icon in the bottom bar of SkEditor's window.
/// </summary>
public class BottomIconData : IBottomIconElement
{

    private bool _initialized;
    private Button? _attachedButton; // will be null if it's a group

    private TextBlock? _attachedTextBlock;
    private IconSourceElement? _attachedIconElement;

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
        _attachedIconElement.IsVisible = IconSource != null;
    }

    public Button? GetButton() => _attachedButton;
    public TextBlock GetTextBlock() => _attachedTextBlock;
    public IconSourceElement? GetIconElement() => _attachedIconElement;
    public bool IsInitialized() => _initialized;

    #region Properties

    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            if (_attachedButton != null)
                _attachedButton.IsEnabled = value;
        }
    }

    private IconSource? _iconSource;
    public IconSource? IconSource
    {
        get => _iconSource;
        set
        {
            _iconSource = value;
            if (_attachedIconElement != null)
            {
                _attachedIconElement.IconSource = value;
                _attachedIconElement.IsVisible = value != null;
            }
        }
    }

    private string? _text;
    public string? Text
    {
        get => _text;
        set
        {
            _text = value;
            if (_attachedTextBlock != null)
            {
                _attachedTextBlock.Text = value;
                _attachedTextBlock.IsVisible = value != null;
            }
        }
    }

    private EventHandler<BottomIconElementClickedEventArgs>? _clicked;
    public EventHandler<BottomIconElementClickedEventArgs>? Clicked
    {
        get => _clicked;
        set
        {
            _clicked = value;
            if (_attachedButton != null)
                _attachedButton.Click += (sender, _) => AddonLoader.HandleAddonMethod(() => value?.Invoke(sender, new BottomIconElementClickedEventArgs(this)));
        }
    }

    public int Order { get; set; } = 0;
    public string Id { get; set; }

    #endregion

}