using System;
using System.Collections.Generic;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models.Types;

public interface INumberSetting
{
    double? MinValue { get; set; }
    double? MaxValue { get; set; }
    string FormatString { get; set; }
    object? Value { get; set; }
}

public partial class NumberSetting<T>(TranslatableString name)
    : SettingElement<NumberSetting<T>>(name), INumberSetting where T : struct, INumber<T>
{
    public (Func<T> Get, Action<T> Set) Property
    {
        get => (field.Get, field.Set);
        set;
    }

    [ObservableProperty]
    public partial T GenericValue { get; set; }

    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }

    public string FormatString { get; set; } = "{0}";

    public object? Value
    {
        get => GenericValue;
        set
        {
            try
            {
                if (value is T typedValue)
                {
                    GenericValue = typedValue;
                }
                else if (value != null)
                {
                    GenericValue = (T)Convert.ChangeType(value, typeof(T));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Failed to convert value to type {typeof(T).Name}.", ex);
            }
        }
    }

    public static NumberSetting<T> Create(TranslatableString name)
    {
        return new NumberSetting<T>(name);
    }

    public NumberSetting<T> WithProperty((Func<T> Get, Action<T> Set) property)
    {
        Property = property;
        Value = property.Get();
        return this;
    }

    public NumberSetting<T> WithMinValue(double? minValue)
    {
        MinValue = minValue;
        return this;
    }

    public NumberSetting<T> WithMaxValue(double? maxValue)
    {
        MaxValue = maxValue;
        return this;
    }

    public NumberSetting<T> WithFormatString(string? formatString = null)
    {
        if (formatString != null)
        {
            FormatString = formatString;
        }
        else
        {
            if (typeof(T) == typeof(int))
            {
                FormatString = "{0:N0}";
            }
            else if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
            {
                FormatString = "{0:N2}";
            }
        }

        return this;
    }

    partial void OnGenericValueChanged(T oldValue, T newValue)
    {
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            Property.Set(newValue);
        }
    }
}