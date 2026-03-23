using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Tabs;

public partial class EditorTab(TranslatableString header, string content) : Tab(header)
{
    public string Content { get; set; } = content;

    public override IRelayCommand CloseCommand => CloseTabCommand;

    [RelayCommand]
    private void CloseTab()
    {
        App.Services.GetRequiredService<INotificationService>().Debug("Close Editor Tab");

        RequestClose();
    }
}