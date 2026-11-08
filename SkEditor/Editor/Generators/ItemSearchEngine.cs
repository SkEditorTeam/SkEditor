using System;
using System.Linq;

namespace SkEditor.Editor.Generators;

public static class ItemSearchEngine
{
    public static int GetMatchScore(string text, string query)
    {
        if (text.Equals(query, StringComparison.OrdinalIgnoreCase))
        {
            return 1000;
        }

        if (text.StartsWith(query, StringComparison.OrdinalIgnoreCase))
        {
            return 500;
        }

        if (text.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 200;
        }
        
        string[] searchTerms =
        [
            .. query
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
        ];

        if (searchTerms.All(term => text.Contains(term, StringComparison.OrdinalIgnoreCase)))
        {
            return 100;
        }

        return 0;
    }
}