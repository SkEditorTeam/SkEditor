using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using SkEditor.Appearance.Icons;
using SkEditor.Core.Localization;

namespace SkEditor.Configuration.Models;

public abstract class SettingElement(BindableReactiveProperty<TranslatableString> name) : ObservableObject
{
    public BindableReactiveProperty<TranslatableString?> Description { get; protected set; } = new(null);
    public StandardIcon? Icon { get; protected set; }

    public BindableReactiveProperty<bool> IsEnabled { get; protected set; } = new(true);
    public BindableReactiveProperty<bool> IsVisible { get; protected set; } = new(true);
    public BindableReactiveProperty<TranslatableString> Name { get; set; } = name;

    public SettingElement? Parent { get; set; }
    public BindableReactiveProperty<bool?> Status { get; protected set; } = new(null);

    public virtual double? Width { get; set; }
}

public abstract class SettingElement<T>(BindableReactiveProperty<TranslatableString> name) : SettingElement(name)
    where T : SettingElement<T>
{
    public T WithIcon(StandardIcon icon)
    {
        Icon = icon;
        return (T)this;
    }

    public T WithDescription(TranslatableString? description)
    {
        Description.Value = description;
        return (T)this;
    }

    public T WithDescription(BindableReactiveProperty<TranslatableString?> description)
    {
        Description = description;
        return (T)this;
    }

    public T WithDescription(Observable<TranslatableString?> description)
    {
        Description = description.ToBindableReactiveProperty();
        return (T)this;
    }

    public T WithVisible(BindableReactiveProperty<bool> visible)
    {
        IsVisible = visible;
        return (T)this;
    }

    public T WithEnabled(BindableReactiveProperty<bool> enabled)
    {
        IsEnabled = enabled;
        return (T)this;
    }

    public T WithStatus(bool? status)
    {
        Status.Value = status;
        return (T)this;
    }

    public T WithStatus(BindableReactiveProperty<bool?> status)
    {
        Status = status;
        return (T)this;
    }

    public T WithStatus(BindableReactiveProperty<bool> status)
    {
        Status = status.Select(b => (bool?)b).ToBindableReactiveProperty();
        return (T)this;
    }
}