using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Providers.Choice;

namespace SkEditor.Models.Settings.Types;

public record OptionItem(object Value, string Display);

public interface IChoiceSetting
{
    IEnumerable<OptionItem> Options { get; }
    object? SelectedItem { get; set; }
}

public partial class ChoiceSetting<T>(TranslatableString name) : SettingElement<ChoiceSetting<T>>(name), IChoiceSetting
{
    public (Func<T> Get, Action<T> Set) Property
    {
        get => (field.Get, field.Set);
        set
        {
            field = value;
            SelectedOption = value.Get();
        }
    }

    public ISettingsChoiceProvider? Provider { get; protected set; }

    [ObservableProperty]
    public partial T? SelectedOption { get; set; }

    public List<T> Options { get; protected set; } = [];

    public List<OptionItem> WrappedOptions { get; private set; } = [];

    IEnumerable<OptionItem> IChoiceSetting.Options => WrappedOptions;

    object? IChoiceSetting.SelectedItem
    {
        get => WrappedOptions.FirstOrDefault(o =>
            EqualityComparer<T>.Default.Equals((T)o.Value, SelectedOption));
        set => SelectedOption = value is OptionItem item ? (T)item.Value : default!;
    }

    public static ChoiceSetting<T> Create(TranslatableString name)
    {
        return new ChoiceSetting<T>(name);
    }

    public ChoiceSetting<T> WithProperty((Func<T> Get, Action<T> Set) property)
    {
        Property = property;
        return this;
    }

    public ChoiceSetting<T> WithProvider(ISettingsChoiceProvider provider)
    {
        if (Property.Get == null || Property.Set == null)
        {
            throw new InvalidOperationException("Property must be set before setting provider.");
        }

        Provider = provider;
        Options = provider.GetValues().Cast<T>().ToList();

        WrappedOptions = Options
            .Where(o => o != null)
            .Select(o => new OptionItem(o!, provider.GetDisplayValue(o!)))
            .ToList();

        SelectedOption = Property.Get();
        return this;
    }

    partial void OnSelectedOptionChanged(T? oldValue, T? newValue)
    {
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue) && newValue is not null)
        {
            Property.Set(newValue);
        }
    }
}