using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;

namespace SkEditor.Models.Tabs;

public partial class EditorTab(string header, string content) : Tab(header)
{
    public string Content { get; set; } = content;
    
    [RelayCommand]
    private void CloseTab()
    {
        App.Services.GetRequiredService<INotificationService>().Debug("Close Editor Tab");
        
        RequestClose();
    }

    public override IRelayCommand CloseCommand => CloseTabCommand;
}