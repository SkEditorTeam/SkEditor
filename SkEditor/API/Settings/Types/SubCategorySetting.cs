using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;

namespace SkEditor.API.Settings.Types;

/// <summary>
/// A setting that contains a list of settings, used to create subcategories in the settings window.
/// </summary>
/// <param name="settings">The settings that are contained in this subcategory.</param>
public class SubCategorySetting(List<Setting> settings) : ISettingType
{
    public object Deserialize(JToken value)
    {
        throw new NotImplementedException();
    }

    public JToken Serialize(object value)
    {
        throw new NotImplementedException();
    }

    public Control CreateControl(object value, Action<object> onChanged)
    {
        throw new NotImplementedException();
    }

    public bool IsSelfManaged { get; }
}