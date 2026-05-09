using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Models.UI;
using Action = System.Action;

namespace SkEditor.Models.Projects;

public class ContextMenuItem
{
    public static ContextMenuItem Separator => new() { IsSeparator = true };

    public string? Header { get; private set; }
    public StandardIcon? Icon { get; private set; }
    public ICommand? Command { get; private set; }
    public bool IsSeparator { get; private set; }
    public bool AllowForOneItemOnly { get; private set; }

    public static ContextMenuItem Item(string header)
    {
        return new ContextMenuItem { Header = header };
    }

    public ContextMenuItem WithIcon(StandardIcon icon)
    {
        Icon = icon;
        return this;
    }

    public ContextMenuItem WithCommand(Action execute)
    {
        Command = new RelayCommand(execute);
        return this;
    }

    public ContextMenuItem AllowOneItemOnly()
    {
        AllowForOneItemOnly = true;
        return this;
    }
}