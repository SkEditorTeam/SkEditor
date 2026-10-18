using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Core.Localization;

namespace SkEditor.Core.Localization;

/// <summary>
///     Represents a string that can be translated based on a translation key.
///     It listens for global language changes and updates its value accordingly.
/// </summary>
public partial class TranslatableString : ObservableObject
{
    private static readonly HashSet<WeakReference<TranslatableString>> Instances = [];
    private static bool _eventSubscribed;

    private object[] _parameters = [];

    [ObservableProperty]
    public partial string Key { get; set; }

    /// <summary>
    ///     Gets the translated value of this instance based on the current language and the translation key. If the instance
    ///     was created with a raw string value, that value will be returned instead.
    /// </summary>
    public string Value
    {
        get
        {
            // if field is not null, then that's a raw value
            if (field != null)
            {
                return _parameters == null || _parameters.Length == 0
                    ? field
                    : string.Format(field, TransformParameters(_parameters));
            }

            ITranslationService translationService = TranslationService.Current;

            if (_parameters == null || _parameters.Length == 0)
            {
                return translationService.Get(EnumKey);
            }

            string[] resolvedParameters = TransformParameters(_parameters);

            return translationService.Get(EnumKey, resolvedParameters);
        }
    }

    private static string[] TransformParameters(object[] objects)
    {
        return objects.Select(p => p switch
        {
            TranslatableString ts => ts.Value,
            TranslationKey tk => TranslationService.Current.Get(tk),
            _ => p?.ToString() ?? string.Empty
        }).ToArray();
    }

    private TranslationKey EnumKey => Enum.Parse<TranslationKey>(Key);

    /// <summary>
    ///     Creates a new instance of <see cref="TranslatableString" /> with the specified translation key.
    /// </summary>
    /// <param name="key">The translation key to use for retrieving the translated string.</param>
    public TranslatableString(TranslationKey key)
    {
        Key = key.ToString();

        lock (Instances)
        {
            Instances.Add(new WeakReference<TranslatableString>(this));

            if (_eventSubscribed)
            {
                return;
            }

            TranslationService.Current?.LanguageChanged += OnGlobalLanguageChanged;
            _eventSubscribed = true;
        }
    }

    /// <summary>
    ///     Creates a new instance of <see cref="TranslatableString" /> with the specified translation key and parameters.
    /// </summary>
    /// <param name="key">The translation key to use for retrieving the translated string.</param>
    /// <param name="parameters">
    ///     The parameters to use for formatting the translated string. These parameters will be passed to
    ///     the translation system and can be used in the translation templates.
    /// </param>
    public TranslatableString(TranslationKey key, params object[]? parameters) : this(key) =>
        _parameters = parameters ?? [];

    private TranslatableString(string rawValue, params object[]? parameters)
    {
        Key = string.Empty;
        Value = rawValue;
        _parameters = parameters ?? [];
    }

    /// <summary>
    ///     Implicitly converts a <see cref="TranslationKey" /> to a <see cref="TranslatableString" /> by creating a new
    ///     instance with the given key.
    /// </summary>
    /// <param name="key">The translation key to convert to a <see cref="TranslatableString" />.</param>
    /// <returns>A new instance of <see cref="TranslatableString" /> initialized with the provided translation key.</returns>
    public static implicit operator TranslatableString(TranslationKey key)
    {
        return new TranslatableString(key);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="TranslatableString" /> with the specified raw string value.
    /// </summary>
    /// <param name="value">
    ///     The raw string value to use for this instance. This value will not be translated and will be
    ///     returned as-is.
    /// </param>
    /// <returns>A new instance of <see cref="TranslatableString" /> with the specified raw string value.</returns>
    public static TranslatableString FromRawString(string value)
    {
        return new TranslatableString(value);
    }

    public static TranslatableString FromRawString(string template, params object[] parameters)
    {
        TranslatableString translatableString = new(template, parameters);
        return translatableString;
    }

    public void UpdateParameters(params object[] parameters)
    {
        _parameters = parameters;
        OnPropertyChanged(nameof(Value));
    }

    public override string ToString()
    {
        return Value;
    }

    private static void OnGlobalLanguageChanged(object? sender, string language)
    {
        lock (Instances)
        {
            Instances.RemoveWhere(wr => !wr.TryGetTarget(out _));

            foreach (WeakReference<TranslatableString> weakRef in Instances)
            {
                if (weakRef.TryGetTarget(out TranslatableString? instance))
                {
                    instance.OnPropertyChanged(nameof(Value));
                }
            }
        }
    }
}