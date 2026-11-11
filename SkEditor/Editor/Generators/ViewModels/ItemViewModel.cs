using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Editor.Generators.ViewModels;

public class ItemViewModel(string id, string name) : ObservableObject, IDisposable
{
    public string Name { get; } = name;
    public string Id { get; } = id;
    public Bitmap? Image { get; init; }

    internal int MatchScore { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Image?.Dispose();
    }
}