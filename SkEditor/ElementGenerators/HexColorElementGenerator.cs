using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using SkEditor.Views.Controls.Editor;

namespace SkEditor.ElementGenerators;

public partial class HexColorElementGenerator(Action<ColorBoxControl> clickAction) : VisualLineElementGenerator
{
    private static readonly Dictionary<string, string> LegacyColorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "&0", "#000000" },
        { "&1", "#0000AA" },
        { "&2", "#00AA00" },
        { "&3", "#00AAAA" },
        { "&4", "#AA0000" },
        { "&5", "#AA00AA" },
        { "&6", "#FFAA00" },
        { "&7", "#AAAAAA" },
        { "&8", "#555555" },
        { "&9", "#5555FF" },
        { "&a", "#55FF55" },
        { "&b", "#55FFFF" },
        { "&c", "#FF5555" },
        { "&d", "#FF55FF" },
        { "&e", "#FFFF55" },
        { "&f", "#FFFFFF" }
    };

    private static readonly Dictionary<string, string> NamedColorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "black", "#000000" },
        { "dark_blue", "#0000AA" },
        { "dark_green", "#00AA00" },
        { "dark_aqua", "#00AAAA" },
        { "dark_red", "#AA0000" },
        { "dark_purple", "#AA00AA" },
        { "gold", "#FFAA00" },
        { "gray", "#AAAAAA" },
        { "grey", "#AAAAAA" },
        { "dark_gray", "#555555" },
        { "dark_grey", "#555555" },
        { "blue", "#5555FF" },
        { "green", "#55FF55" },
        { "aqua", "#55FFFF" },
        { "red", "#FF5555" },
        { "light_purple", "#FF55FF" },
        { "yellow", "#FFFF55" },
        { "white", "#FFFFFF" }
    };

    private static readonly Regex FullColorRegex = new(
        @"(?:<#(?<hex>(?:[0-9a-fA-F]{3}){1,2})>)" +
        @"|(?<legacy>&[0-9a-fA-F])" +
        @"|(?:<(?:color|colour|c):(?<prefixed>#?(?:[0-9a-fA-F]{3}){1,2}|" +
        string.Join("|", NamedColorMap.Keys.Select(Regex.Escape)) + @")>)" +
        @"|(?:<(?<named>" + string.Join("|", NamedColorMap.Keys.Select(Regex.Escape)) + @")>)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex OnlyHexRegex = OnlyHexRegexGen();

    public override int GetFirstInterestedOffset(int startOffset)
    {
        int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
        TextDocument document = CurrentContext.Document;
        string relevantText = document.GetText(startOffset, endOffset - startOffset);
        Match m = FullColorRegex.Match(relevantText);
        return m.Success ? startOffset + m.Index : -1;
    }

    public override VisualLineElement ConstructElement(int offset)
    {
        int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
        string text = CurrentContext.Document.GetText(offset, endOffset - offset);
        Match m = FullColorRegex.Match(text);

        if (m is not { Success: true, Index: 0 })
        {
            return null!;
        }

        ResolvedColor? resolvedColor = ResolveColor(m);
        if (resolvedColor is null || !Color.TryParse(resolvedColor.HexValue, out Color color))
        {
            return null!;
        }

        ColorBoxControl control = new(color, resolvedColor.IsPrefixed, CurrentContext.TextView,
            CurrentContext.GlobalTextRunProperties, clickAction, offset, m.Length);

        TextBlock.SetBaselineOffset(control, CurrentContext.GlobalTextRunProperties.FontRenderingEmSize);
        control.VerticalAlignment = VerticalAlignment.Center;

        return new InlineObjectElement(0, control);
    }

    private static ResolvedColor? ResolveColor(Match m)
    {
        if (m.Groups["hex"].Success)
        {
            string raw = m.Groups["hex"].Value;
            return raw.StartsWith('#') ? new ResolvedColor(raw) : new ResolvedColor($"#{raw}");
        }

        if (m.Groups["legacy"].Success &&
            LegacyColorMap.TryGetValue(m.Groups["legacy"].Value, out string? legacyHex))
        {
            return new ResolvedColor(legacyHex);
        }

        if (m.Groups["prefixed"].Success)
        {
            string val = m.Groups["prefixed"].Value;
            if (OnlyHexRegex.IsMatch(val))
            {
                return val.StartsWith('#') ? new ResolvedColor(val, true) : new ResolvedColor($"#{val}", true);
            }

            if (NamedColorMap.TryGetValue(val, out string? prefixedHex))
            {
                return new ResolvedColor(prefixedHex, true);
            }
        }

        if (m.Groups["named"].Success &&
            NamedColorMap.TryGetValue(m.Groups["named"].Value, out string? namedHex))
        {
            return new ResolvedColor(namedHex);
        }

        return null;
    }

    [GeneratedRegex(@"^#?(?:[0-9a-fA-F]{3}){1,2}$")]
    private static partial Regex OnlyHexRegexGen();

    private record ResolvedColor(string HexValue, bool IsPrefixed = false);
}