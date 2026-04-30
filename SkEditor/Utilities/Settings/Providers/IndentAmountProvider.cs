using System.Collections.Generic;

namespace SkEditor.Utilities.Settings.Providers;

public class IndentAmountProvider : ISettingsProvider
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