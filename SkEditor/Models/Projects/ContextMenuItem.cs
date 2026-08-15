using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Models.UI;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Projects;

public class ContextMenuItem
{
    public bool AllowForOneItemOnly { get; private set; }
    public IRelayCommand? Command { get; private set; }

    public TranslatableString? Header { get; private set; }
    public StandardIcon? Icon { get; private set; }
    public string? InputGestureText { get; private set; }
    public bool IsSeparator { get; private init; }
    public static ContextMenuItem Separator => new() { IsSeparator = true };

    public static ContextMenuItem Item(TranslatableString header)
    {
        return new ContextMenuItem { Header = header };
    }

    public ContextMenuItem WithIcon(StandardIcon icon)
    {
        Icon = icon;
        return this;
    }

    public ContextMenuItem WithCommand(IRelayCommand? command)
    {
        Command = command;
        return this;
    }

    public ContextMenuItem WithInputGesture(string? gestureText)
    {
        InputGestureText = gestureText;
        return this;
    }

    public ContextMenuItem AllowOneItemOnly()
    {
        AllowForOneItemOnly = true;
        return this;
    }

    public static List<ContextMenuItem> CleanUpSeparators(IEnumerable<ContextMenuItem> menuItems)
    {
        List<ContextMenuItem> cleaned = [];
        bool lastWasSeparator = true;

        foreach (ContextMenuItem item in menuItems)
        {
            if (item.IsSeparator)
            {
                if (lastWasSeparator)
                {
                    continue;
                }

                cleaned.Add(item);
                lastWasSeparator = true;
            }
            else
            {
                cleaned.Add(item);
                lastWasSeparator = false;
            }
        }

        if (cleaned.Count > 0 && cleaned[^1].IsSeparator)
        {
            cleaned.RemoveAt(cleaned.Count - 1);
        }

        return cleaned;
    }
}