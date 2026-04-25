using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Tabs;

public partial class SettingsTab() : Tab(TranslatableString.FromRawString("Settings"), Symbol.Settings)
{
    public override IRelayCommand CloseCommand => CloseTabCommand;

    [RelayCommand]
    private void CloseTab()
    {
        App.Services.GetRequiredService<INotificationService>().Debug("Close settings Tab");

        RequestClose();
    }
}