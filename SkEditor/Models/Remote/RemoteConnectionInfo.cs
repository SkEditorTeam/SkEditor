using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Models.Remote;

public partial class RemoteConnectionInfo : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    private string _name = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    private string _host = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    private string _port = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    private string _username = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled), nameof(IsPasswordAuth),
        nameof(IsPrivateKeyAuth))]
    private RemoteAuthMethod _authMethod;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    private string _password = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    private string _keyFilePath = "";

    public bool IsPasswordAuth => AuthMethod == RemoteAuthMethod.Password;
    public bool IsPrivateKeyAuth => AuthMethod == RemoteAuthMethod.PrivateKey;

    public bool ConnectionFieldsFilled =>
        !string.IsNullOrWhiteSpace(Host) &&
        !string.IsNullOrWhiteSpace(Username) &&
        (IsPasswordAuth
            ? !string.IsNullOrWhiteSpace(Password)
            : !string.IsNullOrWhiteSpace(KeyFilePath));

    public bool RequiredFieldsFilled =>
        !string.IsNullOrWhiteSpace(Name) &&
        ConnectionFieldsFilled;
}