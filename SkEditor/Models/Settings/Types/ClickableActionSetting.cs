using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Settings.Types;

public class ClickableActionSetting(TranslatableString name) : SettingElement<ClickableActionSetting>(name)
{
    public IRelayCommand? Command { get; private set; }

    public static ClickableActionSetting Create(TranslatableString name)
    {
        return new ClickableActionSetting(name);
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