using System;

namespace SkEditor.API;

/// <summary>
/// Represent an element of the bottom bar of SkEditor's window.
/// Can either be a single icon (see <see cref="BottomIconData"/>), or a group of icons
/// (see <see cref="BottomIconGroupData"/>).
/// </summary>
public interface IBottomIconElement
{
    public int Order { get; }
}

public class BottomIconElementClickedEventArgs(IBottomIconElement icon) : EventArgs
{
    public IBottomIconElement Icon { get; } = icon;
}