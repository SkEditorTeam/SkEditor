using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace SkEditor.Core.Common;

public class Lazier<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(
    IServiceProvider provider)
    : Lazy<T>(provider.GetRequiredService<T>) where T : notnull;