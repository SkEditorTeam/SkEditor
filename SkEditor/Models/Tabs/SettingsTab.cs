using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;

namespace SkEditor.Models.Tabs;

public partial class SettingsTab(string header) : Tab(header)
{
    [RelayCommand]
    private void CloseTab()
    {
        App.Services.GetRequiredService<INotificationService>().Debug("Close settings Tab");
        
        RequestClose();
    }

    public override IRelayCommand CloseCommand => CloseTabCommand;
}