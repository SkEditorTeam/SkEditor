using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using R3;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Settings.Types;

public class ActionSetting(BindableReactiveProperty<TranslatableString> name) : SettingElement<ActionSetting>(name)
{
    public IRelayCommand? Command { get; private set; }
    public BindableReactiveProperty<TranslatableString?> Text { get; private set; } = new(null);
    public BindableReactiveProperty<bool> IsButtonVisible { get; private set; } = new(true);
    public bool? IsExternal { get; private set; } = false;

    public static ActionSetting Create(TranslatableString name)
    {
        return new ActionSetting(new BindableReactiveProperty<TranslatableString>(name));
    }

    public static ActionSetting Create(BindableReactiveProperty<TranslatableString> name)
    {
        return new ActionSetting(name);
    }

    public ActionSetting WithAction(TranslatableString? text, Action action)
    {
        Text.Value = text;
        Command = new RelayCommand(action);
        return this;
    }

    public ActionSetting WithAction(TranslatableString? text, Func<Task> action)
    {
        Text.Value = text;
        Command = new AsyncRelayCommand(action);
        return this;
    }

    public ActionSetting WithAction(TranslatableString? text, Action<ActionSetting> action)
    {
        Text.Value = text;
        Command = new RelayCommand(() => action(this));
        return this;
    }

    public ActionSetting WithAction(TranslatableString? text, Func<ActionSetting, Task> action)
    {
        Text.Value = text;
        Command = new AsyncRelayCommand(() => action(this));
        return this;
    }

    public ActionSetting WithAction(BindableReactiveProperty<TranslatableString?> text, Action action)
    {
        Text = text;
        Command = new RelayCommand(action);
        return this;
    }

    public ActionSetting WithButtonVisible(BindableReactiveProperty<bool> visible)
    {
        IsButtonVisible = visible;
        return this;
    }

    public ActionSetting AsExternal()
    {
        IsExternal = true;
        return this;
    }
}