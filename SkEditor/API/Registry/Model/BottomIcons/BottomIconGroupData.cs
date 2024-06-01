using System;
using System.Collections.Generic;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace SkEditor.API;

public class BottomIconGroupData : IBottomIconElement
{
    
    public EventHandler<BottomIconElementClickedEventArgs>? Clicked { get; set; }
    public List<BottomIconData> Children { get; set; }
    public int Order { get; }

    public BottomIconGroupData(List<BottomIconData> children, EventHandler<BottomIconElementClickedEventArgs>? clicked = null, int order = 0)
    {
        Children = children;
        Clicked = clicked;
        
        Children.Sort((a, b) => a.Order.CompareTo(b.Order));
        Order = order;
    }
    
    private bool _initialized;
    private Button? _attachedButton; // will be null if it's a group
    
    public void Setup(Button? button)
    {
        _initialized = true;
        _attachedButton = button;
        
        _attachedButton.Click += (sender, _) => Clicked?.Invoke(sender, new BottomIconElementClickedEventArgs(this));
    }
    
    public BottomIconData? GetById(string id) => Children.Find(x => x.Id == id);
    
    public Button? GetButton() => _attachedButton;
    public bool IsInitialized() => _initialized;
}