using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using R3;
using SkEditor.Models.UI;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Settings.Types;

public class ClickableActionSetting(BindableReactiveProperty<TranslatableString> name)
    : SettingElement<ClickableActionSetting>(name)
{
    public IRelayCommand? Command { get; private set; }
    public StandardIcon? ActionIcon { get; protected set; }

    public static ClickableActionSetting Create(TranslatableString name)
    {
        return new ClickableActionSetting(new BindableReactiveProperty<TranslatableString>(name));
    }

    public static ClickableActionSetting Create(BindableReactiveProperty<TranslatableString> name)
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

    public ClickableActionSetting WithActionIcon(StandardIcon icon)
    {
        ActionIcon = icon;
        return this;
    }
}