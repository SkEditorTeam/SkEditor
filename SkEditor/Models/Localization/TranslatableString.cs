using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Services.Localization;

namespace SkEditor.Utilities.Localization;

/// <summary>
///     Represents a string that can be translated based on a translation key.
///     It listens for global language changes and updates its value accordingly.
/// </summary>
public partial class TranslatableString : ObservableObject, IDisposable
{
    private static readonly HashSet<WeakReference<TranslatableString>> Instances = [];
    private static bool _eventSubscribed;

    private readonly string[] _parameters = [];

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

            TranslationService.Current.LanguageChanged += OnGlobalLanguageChanged;
            _eventSubscribed = true;
        }
    }

    // ReSharper disable once UnusedParameter.Local
    private TranslatableString(string rawValue, bool _)
    {
        Key = string.Empty;
        Value = rawValue;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="TranslatableString" /> with the specified translation key as a string.
    /// </summary>
    /// <param name="key">The translation key as a string to use for retrieving the translated string.</param>
    /// <remarks>This constructor should NOT be used for raw string values. Use <see cref="FromRawString(string)" /> instead.</remarks>
    public TranslatableString(string key) : this(Enum.Parse<TranslationKey>(key))
    {
    }

    /// <summary>
    ///     Creates a new instance of <see cref="TranslatableString" /> with the specified translation key and parameters.
    /// </summary>
    /// <param name="key">The translation key to use for retrieving the translated string.</param>
    /// <param name="parameters">
    ///     The parameters to use for formatting the translated string. These parameters will be passed to
    ///     the translation system and can be used in the translation templates.
    /// </param>
    public TranslatableString(TranslationKey key, params string[]? parameters) : this(key) =>
        _parameters = parameters ?? [];

    [ObservableProperty]
    public partial string Key { get; set; }

    private TranslationKey EnumKey => Enum.Parse<TranslationKey>(Key);

    /// <summary>
    ///     Gets the translated value of this instance based on the current language and the translation key. If the instance
    ///     was created with a raw string value, that value will be returned instead.
    /// </summary>
    public string Value
    {
        get
        {
            if (field != null) return field;

            var translationService = TranslationService.Current;

            return _parameters == null || _parameters.Length == 0
                ? translationService.Get(EnumKey)
                : translationService.Get(EnumKey, _parameters);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        TranslationService.Current.LanguageChanged -= OnGlobalLanguageChanged;
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
        return new TranslatableString(value, true);
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