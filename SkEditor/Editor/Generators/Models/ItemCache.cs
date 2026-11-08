using MemoryPack;

namespace SkEditor.Editor.Generators.Models;

[MemoryPackable]
public partial record ItemCache(Item[] Items);