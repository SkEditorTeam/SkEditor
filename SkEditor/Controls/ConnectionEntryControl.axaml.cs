using System.Diagnostics;
using Avalonia.Controls;
using SkEditor.API;
using SkEditor.Utilities;

namespace SkEditor.Controls;

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

        string key = connectionData.OptionKey;
        AppConfig appConfig = SkEditorAPI.Core.GetAppConfig();

        ApiKeyTextBox.Text = appConfig.GetApiKey(key);
        ApiKeyTextBox.TextChanged += (_, _) =>
        {
            appConfig.SetApiKey(key, ApiKeyTextBox.Text);
            appConfig.Save(); 
        };

        Expander.IconSource = connectionData.IconSource;
    }
}