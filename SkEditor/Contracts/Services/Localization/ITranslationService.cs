using System;
using System.Threading.Tasks;
using SkEditor.Utilities.Localization;

namespace SkEditor.Contracts.Services.Localization;

public interface ITranslationService
{
    /// <summary>
    ///     The folder where language files are stored.
    /// </summary>
    public string LanguagesFolder { get; }

    /// <summary>
    ///     Event triggered when the application's language is changed. The event handler receives the new language as a
    ///     parameter.
    /// </summary>
    event EventHandler<string>? LanguageChanged;

    /// <summary>
    ///     Retrieves the translated string for the given key, formatting it with the provided parameters if any.
    /// </summary>
    /// <param name="key">The translation key.</param>
    /// <param name="parameters">Optional parameters to format the translated string.</param>
    /// <returns>The translated and formatted string.</returns>
    public string Get(TranslationKey key, params string?[] parameters);

    public TranslatableString GetObservable(TranslationKey key);
    TranslatableString GetObservable(TranslationKey key, params object[]? parameters);

    /// <summary>
    ///     Changes the application's language to the specified language.
    /// </summary>
    /// <param name="language">The language to change to.</param>
    /// <param name="startup">Whether this change is occurring during application startup.</param>
    Task ChangeLanguageAsync(string language, bool startup = false);
}