using SkEditor.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkEditor.Utilities;
public static class ChangelogChecker
{
    private static string GetVersion() => $"{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Build}";

    private static string GetChangelog()
    {
        return "Welcome to the new version of SkEditor!\n" +
            "This version includes the following changes:\n" +
            "✨ Added a Code Parser utility to the sidebar - you can enable it in the settings.json file!\n";
    }

    public static void Check()
    {
        string version = ApiVault.Get().GetAppConfig().Version;
        if (version == GetVersion()) return;

        ApiVault.Get().ShowMessage($"v{GetVersion()} 🚀", GetChangelog());

        ApiVault.Get().GetAppConfig().Version = GetVersion();
    }
}
