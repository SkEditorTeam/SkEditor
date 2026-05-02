using System.Collections.Generic;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class IndentAmountChoiceProvider : ISettingsChoiceProvider
{
    public IEnumerable<object> GetValues()
    {
        return
        [
            1,
            2,
            3,
            4
        ];
    }
}