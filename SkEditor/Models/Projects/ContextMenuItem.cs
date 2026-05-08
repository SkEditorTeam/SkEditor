using CommunityToolkit.Mvvm.Input;
using SkEditor.Models.UI;

namespace SkEditor.Models.Projects;

public record ContextMenuItem
{
    public static readonly ContextMenuItem Separator = new() { IsSeparator = true };
    public string? Header { get; init; }
    public StandardIcon? Icon { get; init; }
    public IRelayCommand? Command { get; init; }
    public object? CommandParameter { get; init; }
    public bool IsSeparator { get; init; }

    public bool AllowForOneItemOnly { get; init; }
}