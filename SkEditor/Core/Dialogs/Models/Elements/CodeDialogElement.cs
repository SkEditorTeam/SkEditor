using R3;

namespace SkEditor.Core.Dialogs.Models.Elements;

public class CodeDialogElement : DialogElement
{
    public required BindableReactiveProperty<string> Code { get; set; }
    
    public static CodeDialogElement Create(string code)
    {
        return new CodeDialogElement
        {
            Code = new BindableReactiveProperty<string>(code)
        };
    }
}