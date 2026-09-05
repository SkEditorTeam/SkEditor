using System;

namespace SkEditor.Models.Completions;

public readonly ref struct CompletionContext(
    int startOffset,
    int lineOffset,
    int length,
    ReadOnlySpan<char> word)
{
    public int StartOffset { get; } = startOffset;
    public int LineOffset { get; } = lineOffset;
    public int Length { get; } = length;
    public ReadOnlySpan<char> Word { get; } = word;
}