using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Remote;

public partial class RemoteConnectionInfo : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled), nameof(IsPasswordAuth),
        nameof(IsPrivateKeyAuth))]
    public partial RemoteAuthMethod AuthMethod { get; set; }

    public bool ConnectionFieldsFilled =>
        !string.IsNullOrWhiteSpace(Host) &&
        !string.IsNullOrWhiteSpace(Username) &&
        (IsPasswordAuth
            ? !string.IsNullOrWhiteSpace(Password) || IsPasswordSaved
            : !string.IsNullOrWhiteSpace(KeyFilePath));

    [ObservableProperty]
    public partial string? CustomRootPath { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    public partial string Host { get; set; } = "";

    public bool IsPasswordAuth => AuthMethod == RemoteAuthMethod.Password;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordPlaceholder), nameof(ConnectionFieldsFilled),
        nameof(RequiredFieldsFilled))]
    public partial bool IsPasswordSaved { get; set; }

    public bool IsPrivateKeyAuth => AuthMethod == RemoteAuthMethod.PrivateKey;

    public string? KeyFileName => IsPrivateKeyAuth && !string.IsNullOrWhiteSpace(KeyFilePath)
        ? Path.GetFileName(KeyFilePath)
        : null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled), nameof(KeyFileName))]
    public partial string KeyFilePath { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    public partial string Name { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    public partial string Password { get; set; } = "";

    [ObservableProperty]
    public partial string PasswordHash { get; set; } = string.Empty;

    public TranslatableString PasswordPlaceholder => IsPasswordSaved
        ? TranslatableString.FromRawString("●●●●●●●●")
        : TranslationKey.RemoteConnectionFieldPasswordPlaceholder;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    public partial string Port { get; set; } = "";

    public bool RequiredFieldsFilled =>
        !string.IsNullOrWhiteSpace(Name) &&
        ConnectionFieldsFilled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    public partial string Username { get; set; } = "";
}