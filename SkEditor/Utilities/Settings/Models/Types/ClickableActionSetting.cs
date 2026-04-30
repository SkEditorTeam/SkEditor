using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models.Types;

public class ClickableActionSetting(TranslatableString name) : SettingElement(name)
{
    public IRelayCommand? Command { get; private set; }

    public static ClickableActionSetting Create(TranslatableString name)
    {
        return new ClickableActionSetting(name);
    }

    public ClickableActionSetting WithIcon(SettingIcon icon)
    {
        Icon = icon;
        return this;
    }

    public ClickableActionSetting WithDescription(TranslatableString description)
    {
        Description = description;
        return this;
    }

    public ClickableActionSetting WithAction(Action action)
    {
        Command = new RelayCommand(action);
        return this;
    }

    public ClickableActionSetting WithAction(Func<Task> action)
    {
        Command = new AsyncRelayCommand(action);
        return this;
    }
}