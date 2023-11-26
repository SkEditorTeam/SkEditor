using Avalonia;
using Avalonia.Controls;

namespace SkEditor.Controls;
public partial class LoreLineEditor : UserControl
{
	public static readonly StyledProperty<bool> IsDeleteButtonVisibleProperty =
		AvaloniaProperty.Register<LoreLineEditor, bool>(nameof(IsDeleteButtonVisible));

	public bool IsDeleteButtonVisible { get => GetValue(IsDeleteButtonVisibleProperty); set => SetValue(IsDeleteButtonVisibleProperty, value); }

	public LoreLineEditor()
	{
		InitializeComponent();

		DataContext = this;
	}
}
