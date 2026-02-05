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
    
    private readonly string[] _parameters = [];

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
    
    public TranslatableString(string key, params string[]? parameters) : this(key)
    {
        _parameters = parameters ?? [];
    }

    public string Value => _parameters == null || _parameters.Length == 0
        ? Translation.Get(Key)
        : Translation.Get(Key, _parameters);

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
}