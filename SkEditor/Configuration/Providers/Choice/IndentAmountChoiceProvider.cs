using System.Collections.Generic;

namespace SkEditor.Configuration.Providers.Choice;

public class IndentAmountChoiceProvider : ISettingsChoiceProvider<int>
{
    public IEnumerable<int> GetValues()
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