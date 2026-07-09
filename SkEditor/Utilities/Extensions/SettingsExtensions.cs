using R3;
using SkEditor.Utilities.Settings;

namespace SkEditor.Utilities.Extensions;

public static class SettingsExtensions
{
    extension(ExperimentsConfig experiments)
    {
        public BindableReactiveProperty<bool> IsBottomDrawerEnabled()
        {
            return experiments
                .ObservePropertyChanged(x => x.AnalyzerEnabled)
                .CombineLatest(experiments.ObservePropertyChanged(x => x.TerminalEnabled),
                    (analyzer, terminal) => analyzer || terminal
                ).ToBindableReactiveProperty();
        }
    }
}