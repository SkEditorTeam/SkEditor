using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Localization;
using SkEditor.SourceGenerators;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Windows;

public partial class AboutWindowViewModel(
    IClipboardService clipboard,
    ITranslationService translations,
    ILinkService links)
    : ObservableObject
{
    public string OperatingSystem => RuntimeInformation.OSDescription;
    public string FrameworkDescription => RuntimeInformation.FrameworkDescription;
    public string InformationalVersion => App.GetInformationalVersion();
    public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";

    public string VersionString => translations.Get(TranslationKey.AboutVersion, InformationalVersion, AssemblyVersion);

    public DateTime ReleaseDate => BuildInfo.ReleaseDate;
    public string DateString => translations.Get(TranslationKey.AboutReleaseDate, FormatDateNoDayOfWeek(ReleaseDate));

    public string Author => "Notro";
    public string AuthorString => translations.Get(TranslationKey.AboutAuthor, Author);

    [RelayCommand]
    public async Task CopySystemInfo()
    {
        string systemInfo = GetSystemInfoString();
        await clipboard.CopyTextAsync(systemInfo);
    }

    [RelayCommand]
    public void OpenSupport()
    {
        links.OpenLink("https://ko-fi.com/notro");
    }

    [RelayCommand]
    public void OpenAuthor()
    {
        links.OpenLink("https://github.com/NotroDev");
    }

    private static string FormatDateNoDayOfWeek(DateTime date)
    {
        CultureInfo culture = CultureInfo.CurrentCulture;

        string pattern = culture.DateTimeFormat.LongDatePattern;
        string noDay = DayOfWeekRegex().Replace(pattern, "").Trim();
        return date.ToString(noDay, culture);
    }

    private static string GetSystemInfoString()
    {
        StringBuilder sb = new();

        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
        string informational = App.GetInformationalVersion();

        sb.AppendLine("--- SkEditor Debug Information ---");
        sb.AppendLine($"SkEditor Version: {informational} (Assembly Version: {version})");
        sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        sb.AppendLine("--- Runtime Information ---");
        sb.AppendLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
        sb.AppendLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
        sb.AppendLine();

        sb.AppendLine("--- Operating System ---");
        sb.AppendLine($"OS Description: {RuntimeInformation.OSDescription}");
        sb.AppendLine($"OS Architecture: {RuntimeInformation.OSArchitecture}");
        sb.AppendLine($"Platform: {RuntimeInformation.RuntimeIdentifier}");
        sb.AppendLine();

        sb.AppendLine("--- Hardware & Environment ---");
        sb.AppendLine($"Processor Count: {Environment.ProcessorCount}");
        sb.AppendLine($"Memory Usage: {GC.GetTotalMemory(false) / 1024 / 1024} MB");

        return sb.ToString();
    }

    [GeneratedRegex(@"dddd,?\s*")]
    private static partial Regex DayOfWeekRegex();
}