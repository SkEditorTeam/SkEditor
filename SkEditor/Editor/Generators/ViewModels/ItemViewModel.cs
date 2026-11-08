using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Editor.Generators.ViewModels;

public class ItemViewModel(string id, string displayName) : ObservableObject, IDisposable
{
    public string DisplayName { get; } = displayName;
    public string Id { get; } = id;
    public Bitmap? Image { get; init; }

    internal int MatchScore { get; set; }

    public override string ToString()
    {
        return DisplayName;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Image?.Dispose();
    }
}