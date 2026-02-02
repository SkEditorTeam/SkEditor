using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.API;

namespace SkEditor.Utilities.Localization;

public partial class TranslatableString : ObservableObject
{
    private static readonly HashSet<WeakReference<TranslatableString>> Instances = [];
    private static bool _eventSubscribed;
    
    [ObservableProperty]
    private string _key;

    public TranslatableString(string key)
    {
        _key = key;
        
        lock (Instances)
        {
            Instances.Add(new WeakReference<TranslatableString>(this));

            if (_eventSubscribed)
            {
                return;
            }

            SkEditorAPI.Events.OnLanguageChanged += OnGlobalLanguageChanged;
            _eventSubscribed = true;
        }
    }

    public string Value => Translation.Get(Key);

    private static void OnGlobalLanguageChanged(object? sender, EventArgs e)
    {
        lock (Instances)
        {
            Instances.RemoveWhere(wr => !wr.TryGetTarget(out _));
            
            foreach (var weakRef in Instances)
            {
                if (weakRef.TryGetTarget(out var instance))
                {
                    instance.OnPropertyChanged(nameof(Value));
                }
            }
        }
    }

    public static implicit operator string(TranslatableString t) => t.Value;
    public static implicit operator TranslatableString(string key) => new(key);
}