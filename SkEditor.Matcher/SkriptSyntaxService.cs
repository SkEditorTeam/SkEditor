using System.Buffers;
using Avalonia.Platform;
using MemoryPack;
using SkEditor.Matcher.Models;
using SkEditor.Matcher.Registries;

namespace SkEditor.Matcher;

public class SkriptSyntaxService : ISkriptSyntaxService
{
    private readonly SkriptParser _parser = new();
    public TypeRegistry TypeRegistry { get; private set; } = null!;
    public Dictionary<SyntaxElement, List<SyntaxPattern>> PatternMap { get; } = [];

    private static readonly Uri DefaultDataUri = new("avares://SkEditor.Matcher/Assets/skript-data.bin");

    public async Task LoadAsync()
    {
        await Task.Run(() =>
        {
            using Stream stream = AssetLoader.Open(DefaultDataUri);
            LoadFromStream(stream);
        });
    }

    public void LoadFromStream(Stream stream)
    {
        int length = (int)stream.Length;
        Span<byte> span = ArrayPool<byte>.Shared.Rent(length).AsSpan(0, length);
        stream.ReadExactly(span);

        SkriptDataDto? dto = MemoryPackSerializer.Deserialize<SkriptDataDto>(span);
        
        if (dto == null)
        {
            throw new InvalidOperationException("Failed to deserialize Skript data.");
        }

        TypeRegistry = new TypeRegistry(dto.ClassInfos);

        PatternMap.Clear();
        foreach (SyntaxElementDto syntaxDto in dto.SyntaxElements)
        {
            SyntaxElement element = SyntaxElement.FromDto(syntaxDto, TypeRegistry);
            List<SyntaxPattern> parsedPatterns = syntaxDto.Patterns
                .Select(p => new SyntaxPattern(_parser.Parse(p.AsSpan(), element, TypeRegistry)))
                .ToList();

            PatternMap[element] = parsedPatterns;
        }
    }

    public IEnumerable<(SyntaxElement Element, SyntaxPattern Pattern)> GetSyntaxByType(SyntaxElementDto.SyntaxType type)
    {
        foreach ((SyntaxElement element, List<SyntaxPattern> patterns) in PatternMap)
        {
            if (element.Type != type)
            {
                continue;
            }

            foreach (SyntaxPattern pattern in patterns)
            {
                yield return (element, pattern);
            }
        }
    }
}