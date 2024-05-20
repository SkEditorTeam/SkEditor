using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using SkEditor.API;

namespace SkEditor.Controls;

public partial class ConnectionEntryControl : UserControl
{
    
    public ConnectionEntryControl(string name, string url, string key,
        string? description = null)
    {
        InitializeComponent();
        
        Expander.Header = name;
        Expander.Description = description ?? "";
        
        OpenDashboardButton.Click += (_, _) =>
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        };
        
        ApiKeyTextBox.Text = ApiVault.Get().GetAppConfig().GetOptionValue<string>(key);
        ApiKeyTextBox.TextChanged += (_, _) =>
        {
            ApiVault.Get().GetAppConfig().SetOptionValue(key, ApiKeyTextBox.Text);
        };

        Expander.IconSource = new BitmapIconSource()
        {
            UriSource = new("avares://SkEditor/Assets/Brands/" + name + ".png")
        };
    }
    
}