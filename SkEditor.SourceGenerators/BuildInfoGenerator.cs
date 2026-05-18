using System;
using Microsoft.CodeAnalysis;

namespace SkEditor.SourceGenerators;

[Generator]
public class BuildInfoGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, (ctx, _) =>
        {
            string source = $$"""
                              namespace SkEditor.SourceGenerators
                              {
                                  public static class BuildInfo
                                  {
                                      public static readonly System.DateTime ReleaseDate = new System.DateTime({{DateTime.Now:yyyy, M, d}});
                                  }
                              }
                              """;

            ctx.AddSource("BuildInfo.g.cs", source);
        });
    }
}