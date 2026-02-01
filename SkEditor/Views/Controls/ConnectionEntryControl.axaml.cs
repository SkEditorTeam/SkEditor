using System;
using System.Diagnostics;
using Avalonia.Controls;
using SkEditor.API;
using SecretsConfig = SkEditor.Utilities.Settings.Configs.SecretsConfig;

namespace SkEditor.Views.Controls;

public partial class ConnectionEntryControl : UserControl
{
    public ConnectionEntryControl(ConnectionData connectionData)
    {
        InitializeComponent();

        Expander.Header = connectionData.Name;
        Expander.Description = connectionData.Description;

        string? dashboardUrl = connectionData.DashboardUrl;
        if (dashboardUrl == null)
        {
            OpenDashboardButton.IsVisible = false;
        }
        else
        {
            OpenDashboardButton.Click += (_, _) =>
            {
                Process.Start(new ProcessStartInfo(dashboardUrl)
                {
                    UseShellExecute = true
                });
            };
        }

        SecretsConfig secretsConfig = SkEditorAPI.Configs.GetSecretsConfig();

        ApiKeyTextBox.Text = connectionData.GetApiKey();
        ApiKeyTextBox.TextChanged += async (_, _) =>
        {
            connectionData.SetApiKey(ApiKeyTextBox.Text);
            await secretsConfig.Save();
        };

        Expander.IconSource = connectionData.IconSource;
    }

    [Obsolete("This constructor is for designer use only.", true)]
    public ConnectionEntryControl() => InitializeComponent();
}