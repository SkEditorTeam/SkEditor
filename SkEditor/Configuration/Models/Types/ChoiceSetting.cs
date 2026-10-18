using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using R3;
using SkEditor.Configuration.Providers.Choice;
using SkEditor.Core.Localization;

namespace SkEditor.Configuration.Models.Types;

public record OptionItem(object? Value, TranslatableString Display)
{
    public override string ToString()
    {
        return Display?.Value ?? string.Empty;
    }
}

public interface IChoiceSetting
{
    ObservableCollection<OptionItem> Options { get; }
    object? SelectedItem { get; set; }
    double? Width { get; set; }
    double? MaxWidth { get; set; }
}

public class ChoiceSetting<T> : SettingElement<ChoiceSetting<T>>, IChoiceSetting, IDisposable
{
    private DisposableBag _disposables;
    private IDisposable? _providerSubscription;
    private bool _isRefreshing;

    public ChoiceSetting(BindableReactiveProperty<TranslatableString> name) : base(name) =>
        SelectedOption
            .Subscribe(OnSelectedOptionChanged)
            .AddTo(ref _disposables);

    public List<T> Options { get; protected set; } = [];

    public (Func<T> Get, Action<T> Set) Property
    {
        get => (field.Get, field.Set);
        set
        {
            field = value;
            SelectedOption.Value = value.Get();
        }
    }

    public ISettingsChoiceProvider<T>? Provider { get; protected set; }

    public BindableReactiveProperty<T> SelectedOption { get; } = new(default!);

    public ObservableCollection<OptionItem> WrappedOptions { get; } = [];

    ObservableCollection<OptionItem> IChoiceSetting.Options => WrappedOptions;

    object? IChoiceSetting.SelectedItem
    {
        get => WrappedOptions.FirstOrDefault(o =>
            EqualityComparer<T>.Default.Equals((T?)o.Value, SelectedOption.Value));
        set => SelectedOption.Value = value is OptionItem { Value: T val } ? val : default!;
    }

    public override double? Width { get; set; }
    public double? MinWidth { get; set; }
    public double? MaxWidth { get; set; }

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

    public ChoiceSetting<T> WithWidth(double width)
    {
        Width = width;
        return this;
    }

    public ChoiceSetting<T> WithMinWidth(double minWidth)
    {
        MinWidth = minWidth;
        return this;
    }

    public ChoiceSetting<T> WithMaxWidth(double maxWidth)
    {
        MaxWidth = maxWidth;
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

        SelectedOption.Value = Property.Get();
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
                WrappedOptions.Add(new OptionItem(
                    o,
                    Provider.GetDisplayValue(o!)));
            }

            T currentVal = Property.Get != null ? Property.Get() : SelectedOption.Value;

            if (!Options.Contains(currentVal) && Options.Count > 0)
            {
                currentVal = Options[0];
            }

            SelectedOption.Value = currentVal;
            OnPropertyChanged(nameof(IChoiceSetting.SelectedItem));
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void OnSelectedOptionChanged(T newValue)
    {
        if (_isRefreshing || Property.Set == null || Property.Get == null)
        {
            return;
        }

        if (!EqualityComparer<T>.Default.Equals(Property.Get(), newValue))
        {
            Property.Set(newValue);
        }

        OnPropertyChanged(nameof(IChoiceSetting.SelectedItem));
    }

    public void Dispose()
    {
        _providerSubscription?.Dispose();
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}