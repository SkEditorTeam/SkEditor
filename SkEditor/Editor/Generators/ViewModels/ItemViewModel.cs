using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Editor.Generators.ViewModels;

public class ItemViewModel(string id, string displayName) : ObservableObject, IDisposable
{
    public static readonly ItemViewModel None = new(string.Empty, "None")
    {
        Image = null
    };
    
    public string DisplayName { get; } = displayName;
    public string Id { get; } = id;
    public Bitmap? Image { get; init; }

    internal int MatchScore { get; set; }
    
    internal bool IsNone => ReferenceEquals(this, None);

    public override string ToString()
    {
        return DisplayName;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        if (IsNone) return;
        Image?.Dispose();
    }
}