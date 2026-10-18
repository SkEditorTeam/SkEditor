using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;
using FluentIcons.Common;
using SkEditor.Core.Localization;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Welcome.Models;

[method: JsonConstructor]
public sealed class WelcomeScreenRecentItem(string name, string path, bool isFolder, DateTime lastOpened)
    : INotifyPropertyChanged
{
    [JsonIgnore]
    public string DisplayPath => IsFolder ? Path : System.IO.Path.GetDirectoryName(Path) ?? string.Empty;

    [JsonIgnore]
    public Symbol IconSymbol => IsFolder ? Symbol.Folder : Symbol.Document;

    public bool IsFolder { get; } = isFolder;
    public DateTime LastOpened { get; } = lastOpened;
    public string Name { get; } = name;
    public string Path { get; } = path;

    [JsonIgnore]
    public TranslatableString PrettyLastOpened => ToPrettyDate(LastOpened);

    public static TranslatableString ToPrettyDate(DateTime date)
    {
        CultureInfo appCulture = CultureInfo.CurrentCulture;
        TimeSpan span = DateTime.Now - date;

        switch (span.TotalDays)
        {
            case < 0:
                return new TranslatableString(TranslationKey.WelcomePageRecentInTheFuture);
            case >= 365 * 50:
                return new TranslatableString(TranslationKey.WelcomePageRecentTimeTraveler);
        }

        if (span.TotalMinutes < 1)
        {
            return new TranslatableString(TranslationKey.WelcomePageRecentJustNow);
        }

        if (span.TotalHours < 1)
        {
            int minutes = (int)span.TotalMinutes;

            return new TranslatableString(GetKey("WelcomePageRecentMinutes", GetPluralKey(minutes)),
                minutes.ToString());
        }

        if (!(span.TotalDays < 1))
        {
            return date.Date == DateTime.Today.AddDays(-1)
                ? new TranslatableString(TranslationKey.WelcomePageRecentYesterdayAt,
                    date.ToString("t", appCulture))
                : TranslatableString.FromRawString(date.ToString("g", appCulture));
        }

        int hours = (int)span.TotalHours;
        return new TranslatableString(GetKey("WelcomePageRecentHours", GetPluralKey(hours)), hours.ToString());
    }

    public static string GetPluralKey(int n)
    {
        if (n == 1)
        {
            return "One";
        }

        if (n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20))
        {
            return "Few";
        }

        return "Many";
    }

    public static TranslationKey GetKey(string prefix, string pluralKey)
    {
        string keyName = $"{prefix}{pluralKey}";
        return Enum.TryParse(keyName, out TranslationKey key)
            ? key
            : throw new ArgumentException($"No translation key found for {keyName}");
    }

    public void RefreshTime()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrettyLastOpened)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}