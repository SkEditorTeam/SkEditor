using System;
using System.Collections.Generic;
using System.Linq;
using SkEditor.Models.Documentation;

namespace SkEditor.Utilities.Docs;

public static class DocumentationSearchEngine
{
    public static List<DocumentationEntry>? Filter(string searchText,
        IReadOnlyList<DocumentationEntry>? documentationEntries,
        DocumentationSyntaxType selectedSyntaxType, string? selectedAddon)
    {
        List<DocumentationEntry>? filteredEntries;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            filteredEntries = documentationEntries?
                .Where(entry => selectedSyntaxType == DocumentationSyntaxType.All || entry.Type == selectedSyntaxType)
                .Where(entry =>
                    selectedAddon == null ||
                    entry.Addon.Equals(selectedAddon, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return filteredEntries;
        }

        string[] searchTerms = searchText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToArray();

        string[] normalTerms = searchTerms
            .Where(t => !t.Contains(':'))
            .ToArray();

        filteredEntries = documentationEntries?
            .Where(entry => selectedSyntaxType == DocumentationSyntaxType.All || entry.Type == selectedSyntaxType)
            .Where(entry => selectedAddon == null ||
                            entry.Addon.Equals(selectedAddon, StringComparison.OrdinalIgnoreCase))
            .Where(entry => CheckAdvancedMatch(entry, searchTerms))
            .Select(entry => new
            {
                Entry = entry,
                Score = normalTerms.Length > 0 ? CalculateScore(entry, searchText, normalTerms) : 1
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Entry)
            .ToList();

        return filteredEntries;
    }

    private static bool CheckAdvancedMatch(DocumentationEntry entry, string[] terms)
    {
        bool success = true;

        foreach (ReadOnlySpan<char> term in terms)
        {
            int colonIndex = term.IndexOf(':');

            if (colonIndex <= 0)
            {
                continue;
            }

            ReadOnlySpan<char> tag = term[..colonIndex];
            ReadOnlySpan<char> value = term[(colonIndex + 1)..];

            if (tag.Equals("addon", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(entry.Addon) &&
                    entry.Addon.Contains(value, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                success = false;
                break;
            }

            if (tag.Equals("version", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(entry.Version) &&
                    entry.Version.Contains(value, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                success = false;
                break;
            }

            if (tag.Equals("type", StringComparison.OrdinalIgnoreCase))
            {
                if (entry.Type.ToString().Contains(value, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                success = false;
                break;
            }

            if (tag.Equals("description", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(entry.Description) &&
                    entry.Description.Contains(value, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                success = false;
                break;
            }

            if (tag.Equals("patterns", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(entry.Patterns) &&
                    entry.Patterns.Contains(value, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            if (tag.Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                if (entry.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                success = false;
                break;
            }
        }

        return success;
    }

    private static int CalculateScore(DocumentationEntry entry, string fullSearchText, string[] terms)
    {
        int score = 0;

        if (entry.Name.Equals(fullSearchText, StringComparison.OrdinalIgnoreCase))
        {
            score += 200;
        }

        foreach (string term in terms)
        {
            int termScore = 0;

            if (entry.Name.Equals(term, StringComparison.OrdinalIgnoreCase))
            {
                termScore += 100;
            }
            else if (entry.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                termScore += 50;
            }

            if (!string.IsNullOrEmpty(entry.Patterns) &&
                entry.Patterns.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                termScore += 40;
            }

            if (!string.IsNullOrEmpty(entry.Description) &&
                entry.Description.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                termScore += 10;
            }

            if (termScore == 0)
            {
                return 0;
            }

            score += termScore;
        }

        return score;
    }
}