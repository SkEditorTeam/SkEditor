using R3;

namespace SkEditor.Core.Dialogs.Models.Elements;

public class ProgressDialogElement : DialogElement
{
    public required BindableReactiveProperty<float> Value { get; set; }
    public BindableReactiveProperty<bool> IsIndeterminate { get; set; } = new();
}