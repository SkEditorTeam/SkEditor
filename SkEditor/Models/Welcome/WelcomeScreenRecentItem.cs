using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Welcome;

public class WelcomeScreenRecentItem(string name, string path, bool isFolder, DateTime lastOpened)
    : ObservableObject
{
    public string Name { get; } = name;
    public string Path { get; } = path;
    public bool IsFolder { get; } = isFolder;
    public DateTime LastOpened { get; } = lastOpened;

    public Symbol IconSymbol => IsFolder ? Symbol.Folder : Symbol.Document;
    public string DisplayPath => IsFolder ? Path : System.IO.Path.GetDirectoryName(Path) ?? string.Empty;

    public TranslatableString PrettyLastOpened => ToPrettyDate(LastOpened);

    public void RefreshTime()
    {
        OnPropertyChanged(nameof(PrettyLastOpened));
    }

    public static TranslatableString ToPrettyDate(DateTime date)
    {
        CultureInfo appCulture = CultureInfo.CurrentCulture;
        TimeSpan span = DateTime.Now - date;

        switch (span.TotalDays)
        {
            case < 0:
                return Translation.GetObservable(TranslationKey.WelcomePageRecentInTheFuture);
            case >= 365 * 50:
                return Translation.GetObservable(TranslationKey.WelcomePageRecentTimeTraveler);
        }

        if (span.TotalMinutes < 1)
        {
            return Translation.GetObservable(TranslationKey.WelcomePageRecentJustNow);
        }

        if (span.TotalHours < 1)
        {
            int minutes = (int)span.TotalMinutes;

            return Translation.GetObservable(GetKey("WelcomePageRecentMinutes", GetPluralKey(minutes)),
                minutes.ToString());
        }

        if (!(span.TotalDays < 1))
        {
            return date.Date == DateTime.Today.AddDays(-1)
                ? Translation.GetObservable(GetKey("WelcomePageRecentYesterdayAt", GetPluralKey(1)),
                    date.ToString("t", appCulture))
                : TranslatableString.FromRawString(date.ToString("g", appCulture));
        }

        int hours = (int)span.TotalHours;
        return Translation.GetObservable(GetKey("WelcomePageRecentHours", GetPluralKey(hours)), hours.ToString());
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
}