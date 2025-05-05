using System;
using System.Collections.Generic;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json.Linq;
using SkEditor.Views;
using SkEditor.Views.Settings;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.API.Settings.Types;

/// <summary>
///     A setting that contains a list of settings, used to create subcategories in the settings window.
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
        return new IconSourceElement
        {
            IconSource = new SymbolIconSource { Symbol = Symbol.ArrowRight }
        };
    }

    public void SetupExpander(SettingsExpander expander, Setting setting)
    {
        expander.IsClickEnabled = true;
        expander.Click += (_, _) =>
        {
            CustomAddonSettingsPage.Load(setting.Addon, settings);
            SettingsWindow.NavigateToPage(typeof(CustomAddonSettingsPage));
        };
    }

    public bool IsSelfManaged => true;
}