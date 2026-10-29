using R3;

namespace SkEditor.Core.Common;

public static class ReactivePropertyExtensions
{
    extension(BindableReactiveProperty<bool> source)
    {
        public BindableReactiveProperty<bool> Inverse()
        {
            return source.Select(x => !x).ToBindableReactiveProperty();
        }
    }
}