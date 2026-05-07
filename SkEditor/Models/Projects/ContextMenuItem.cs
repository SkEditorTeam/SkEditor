using CommunityToolkit.Mvvm.Input;
using SkEditor.Models.UI;

namespace SkEditor.Models.Projects;

public class ContextMenuItem
{
    public string? Header { get; init; }
    public StandardIcon? Icon { get; init; }
    public IRelayCommand? Command { get; init; }
    public object? CommandParameter { get; init; }
    public bool IsSeparator { get; init; }
    
    public static ContextMenuItem Separator() => new() { IsSeparator = true };
}