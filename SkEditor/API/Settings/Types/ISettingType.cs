using System;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;

namespace SkEditor.API.Settings.Types;

/// <summary>
/// Serialize and deserialize a setting value. Feel free to
/// implement your own setting types if the default ones
/// don't fit your needs.
/// </summary>
public interface ISettingType
{
    
    /// <summary>
    /// Deserialize a setting value from a JSON object. The given
    /// object is either a <see cref="JObject"/>, a <see cref="JArray"/> or a <see cref="JValue"/>.
    /// </summary>
    /// <param name="value">The JSON object/array/value to deserialize.</param>
    /// <returns>The deserialized value.</returns>
    public object Deserialize(JToken value);
    
    /// <summary>
    /// Serialize a setting value to a JSON object. The returned
    /// value must be either a <see cref="JObject"/>, a <see cref="JArray"/> or a <see cref="JValue"/>.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized JSON object/array/value.</returns>
    public JToken Serialize(object value);
    
    /// <summary>
    /// Create a user control to edit the setting value.
    /// </summary>
    /// <param name="value">The current value of the setting.</param>
    /// <param name="onChanged">The action to call when the value changes.</param>
    /// <returns>The created control.</returns>
    public Control CreateControl(object value, Action<object> onChanged);
    
    /// <summary>
    /// Check if this type is self-managed, meaning SkEditor
    /// will not handle setting save and change events.
    /// </summary>
    public bool IsSelfManaged { get; }
    
}