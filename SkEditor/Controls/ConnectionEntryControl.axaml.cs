using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using System;
using System.Diagnostics;
using System.IO;

namespace SkEditor.Controls;

public partial class ConnectionEntryControl : UserControl
{

    public ConnectionEntryControl(string name, string icon, string url, string key,
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

        Stream stream = AssetLoader.Open(new Uri("avares://SkEditor/Assets/Brands/" + icon));
        Expander.IconSource = new ImageIconSource()
        {
            Source = icon.EndsWith(".svg") ? LoadSvgIcon(stream) : LoadPngIcon(stream)
        };
    }

    private static SvgImage LoadSvgIcon(Stream stream)
    {
        return new SvgImage
        {
            Source = SvgSource.LoadFromStream(stream)
        };
    }

    private static Bitmap LoadPngIcon(Stream stream)
    {
        return new Bitmap(stream);
    }
}