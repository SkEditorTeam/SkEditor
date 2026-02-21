using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit.Utils;

namespace SkEditor.Views.Pages;

public partial class WelcomePage : UserControl
{
    public WelcomePage()
    {
        InitializeComponent();

        RecentItemsPanel.GetObservable(IsVisibleProperty)
            .Subscribe(UpdateGridLayout);
    }

    private void UpdateGridLayout(bool panelVisible)
    {
        if (panelVisible)
        {
            MainGrid.ColumnDefinitions[0].Width = new GridLength(450, GridUnitType.Pixel);
            MainGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        }
        else
        {
            MainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            MainGrid.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Pixel);
        }
    }
}