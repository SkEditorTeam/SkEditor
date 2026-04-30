using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models.Types;

public class ActionSetting(TranslatableString name) : SettingElement(name)
{
    public IRelayCommand? Command { get; private set; }
    public TranslatableString? Text { get; private set; }

    public static ActionSetting Create(TranslatableString name)
    {
        return new ActionSetting(name);
    }

    public ActionSetting WithIcon(SettingIcon icon)
    {
        Icon = icon;
        return this;
    }

    public ActionSetting WithDescription(TranslatableString description)
    {
        Description = description;
        return this;
    }

    public ActionSetting WithAction(TranslatableString text, Action action)
    {
        Text = text;
        Command = new RelayCommand(action);
        return this;
    }

    public ActionSetting WithAction(TranslatableString text, Func<Task> action)
    {
        Text = text;
        Command = new AsyncRelayCommand(action);
        return this;
    }
}