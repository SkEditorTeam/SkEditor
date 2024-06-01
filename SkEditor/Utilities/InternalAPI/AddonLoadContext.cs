using System;
using System.Reflection;
using System.Runtime.Loader;

namespace SkEditor.Utilities.InternalAPI;

public class AddonLoadContext(string pluginPath) : AssemblyLoadContext(true)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath != null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }
    
}