using System.Buffers;
using Avalonia.Platform;
using MemoryPack;
using SkEditor.Matcher.Models;
using SkEditor.Matcher.Registries;

namespace SkEditor.Matcher;

public class SkriptSyntaxService : ISkriptSyntaxService
{
    private static readonly Uri DefaultDataUri = new("avares://SkEditor.Matcher/Assets/skript-data.bin");
    private readonly SkriptParser _parser = new();
    public Dictionary<SyntaxElement, List<SyntaxPattern>> PatternMap { get; } = [];
    public TypeRegistry TypeRegistry { get; private set; } = null!;

    private readonly TaskCompletionSource _initTcs = new();
    public Task InitializationTask => _initTcs.Task;

    public void Load()
    {
        try
        {
            using Stream stream = AssetLoader.Open(DefaultDataUri);
            LoadFromStream(stream);
            _initTcs.TrySetResult();
        }
        catch (Exception ex)
        {
            _initTcs.TrySetException(ex);
            throw;
        }
    }

    public void LoadFromStream(Stream stream)
    {
        int length = (int)stream.Length;
        byte[] rented = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            Span<byte> span = rented.AsSpan(0, length);
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
                List<SyntaxPattern> parsedPatterns = new(syntaxDto.Patterns.Length);
                foreach (string p in syntaxDto.Patterns)
                {
                    parsedPatterns.Add(new SyntaxPattern(_parser.Parse(p.AsSpan(), element, TypeRegistry)));
                }

                PatternMap[element] = parsedPatterns;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
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