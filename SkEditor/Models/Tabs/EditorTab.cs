using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;

namespace SkEditor.Models.Tabs;

public partial class EditorTab(string header, TextEditor textEditor) : Tab(header, textEditor)
{
    [RelayCommand]
    private void CloseTab()
    {
        App.Services.GetRequiredService<INotificationService>().Debug("Close Editor Tab");
        
        RequestClose();
    }

    public override IRelayCommand CloseCommand => CloseTabCommand;
}