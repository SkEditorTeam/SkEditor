﻿using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;

namespace SkEditor.API.Settings.Types;

public class ComboBoxSetting(string[] items, int minWidth = 100) : ISettingType
{
    
    public List<string> Items { get; } = [..items];
    public int MinWidth { get; } = minWidth;

    public object Deserialize(JToken value)
    {
        return value.ToObject<string>();
    }

    public JToken Serialize(object value)
    {
        return JToken.FromObject(value);
    }

    public Control CreateControl(object value, Action<object> onChanged)
    {
        var comboBox = new ComboBox { MinWidth = MinWidth };
        
        foreach (var item in Items)
            comboBox.Items.Add(new ComboBoxItem { Content = item, Tag = item });
        
        comboBox.SelectedItem = Items.Contains(value as string) ? value : Items[0];
        comboBox.SelectionChanged += (_, _) => onChanged((comboBox.SelectedItem as ComboBoxItem)?.Tag);
        
        return comboBox;
    }
    
    public bool IsSelfManaged => false;
}