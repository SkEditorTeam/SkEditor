using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Providers.Choice;

namespace SkEditor.Models.Settings.Types;

public record OptionItem(object? Value, TranslatableString Display);

public interface IChoiceSetting
{
    ObservableCollection<OptionItem> Options { get; }
    object? SelectedItem { get; set; }
}

public partial class ChoiceSetting<T>(BindableReactiveProperty<TranslatableString> name)
    : SettingElement<ChoiceSetting<T>>(name), IChoiceSetting, IDisposable
{
    private IDisposable? _providerSubscription;
    private bool _isRefreshing;
    
    public List<T> Options { get; protected set; } = [];

    public (Func<T> Get, Action<T> Set) Property
    {
        get => (field.Get, field.Set);
        set
        {
            field = value;
            SelectedOption = value.Get();
        }
    }

    public ISettingsChoiceProvider<T>? Provider { get; protected set; }

    [ObservableProperty]
    public partial T SelectedOption { get; set; }

    public ObservableCollection<OptionItem> WrappedOptions { get; } = [];

    ObservableCollection<OptionItem> IChoiceSetting.Options => WrappedOptions;

    object? IChoiceSetting.SelectedItem
    {
        get => WrappedOptions.FirstOrDefault(o =>
            EqualityComparer<T>.Default.Equals((T?)o.Value, SelectedOption));
        set => SelectedOption = value is OptionItem { Value: T val } ? val : default!;
    }

    public static ChoiceSetting<T> Create(TranslatableString name)
    {
        return new ChoiceSetting<T>(new BindableReactiveProperty<TranslatableString>(name));
    }

    public static ChoiceSetting<T> Create(BindableReactiveProperty<TranslatableString> name)
    {
        return new ChoiceSetting<T>(name);
    }

    public ChoiceSetting<T> WithProperty((Func<T> Get, Action<T> Set) property)
    {
        Property = property;
        return this;
    }

    public ChoiceSetting<T> WithProvider(ISettingsChoiceProvider<T> provider)
    {
        if (Property.Get == null || Property.Set == null)
        {
            throw new InvalidOperationException("Property must be set before setting provider.");
        }

        Provider = provider;
        _providerSubscription?.Dispose();

        RefreshOptions();

        if (provider.ValuesChanged is not null)
        {
            _providerSubscription = provider.ValuesChanged
                .Subscribe(_ => RefreshOptions());
        }

        SelectedOption = Property.Get();
        return this;
    }

    public void RefreshOptions()
    {
        if (Provider is null)
        {
            return;
        }

        _isRefreshing = true;
        try
        {
            Options = Provider.GetValues().ToList();

            WrappedOptions.Clear();
            foreach (T o in Options)
            {
                WrappedOptions.Add(new OptionItem(o, Provider.GetDisplayValue(o!)));
            }

            T currentVal = Property.Get != null ? Property.Get() : SelectedOption;

            if (!Options.Contains(currentVal) && Options.Count > 0)
            {
                currentVal = Options[0];
            }

            SelectedOption = currentVal;
            OnPropertyChanged(nameof(IChoiceSetting.SelectedItem));
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    partial void OnSelectedOptionChanged(T oldValue, T newValue)
    {
        if (_isRefreshing)
        {
            return;
        }

        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            Property.Set(newValue);
        }

        OnPropertyChanged(nameof(IChoiceSetting.SelectedItem));
    }

    public void Dispose()
    {
        _providerSubscription?.Dispose();
        GC.SuppressFinalize(this);
    }
}