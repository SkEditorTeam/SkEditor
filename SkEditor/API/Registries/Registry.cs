using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkEditor.API;

/// <summary>
///     Represent a registry, which is a collection of key-value pairs,
///     each being linked with a registry key (= the IAddon instance).
/// </summary>
public class Registry<TValue> : IEnumerable<TValue>
{
    private readonly Dictionary<RegistryKey, TValue> _registry = new();

    public IEnumerator<TValue> GetEnumerator()
    {
        return _registry.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Get the value associated with the given key.
    /// </summary>
    /// <param name="key">The key to look for.</param>
    /// <returns>The value associated with the key, or the default value if the key is not found.</returns>
    public TValue? GetValue(RegistryKey key)
    {
        return _registry.GetValueOrDefault(key);
    }

    /// <summary>
    ///     Get all values registered by the specified <see cref="IAddon" />.
    /// </summary>
    /// <param name="addon">The addon to get the values for.</param>
    /// <returns>An IEnumerable of all values registered by the addon.</returns>
    public IEnumerable<TValue> GetValues(IAddon addon)
    {
        return _registry.Where(pair => pair.Key.Addon == addon).Select(pair => pair.Value);
    }

    /// <summary>
    ///     Registers a new key-value pair in the registry.
    /// </summary>
    /// <param name="key">The key to register.</param>
    /// <param name="value">The value to associate with the key.</param>
    /// <returns>True if the key-value pair was added successfully, false if the key already exists.</returns>
    public bool Register(RegistryKey key, TValue value)
    {
        return _registry.TryAdd(key, value);
    }

    /// <summary>
    ///     Checks if a key exists in the registry.
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>True if the key exists, false otherwise.</returns>
    public bool HasKey(RegistryKey key)
    {
        return _registry.ContainsKey(key);
    }

    /// <summary>
    ///     Checks if a full key exists in the registry.
    ///     The full key is represented by the addon ID and the key ID, separated by a slash.
    /// </summary>
    /// <param name="fullKey">The full key to check for.</param>
    /// <returns>True if the full key exists, false otherwise.</returns>
    public bool HasFullKey(string fullKey)
    {
        return _registry.Keys.Any(key => key.FullKey == fullKey);
    }

    /// <summary>
    ///     Retrieves all values in the registry.
    /// </summary>
    /// <returns>An IEnumerable of all values in the registry.</returns>
    public IEnumerable<TValue> GetValues()
    {
        return _registry.Values;
    }

    /// <summary>
    ///     Get the key for a specified value.
    /// </summary>
    /// <param name="value">The value to look for.</param>
    /// <returns>The key associated with the value, or null if the value is not found.</returns>
    public RegistryKey? GetValueKey(TValue value)
    {
        return _registry.FirstOrDefault(pair => pair.Value?.Equals(value) == true).Key;
    }

    public void Unload(IAddon addon)
    {
        foreach (RegistryKey key in _registry.Keys)
        {
            if (key.Addon == addon)
            {
                _registry.Remove(key);
            }
        }
    }
}