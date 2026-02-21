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
    
    private TranslationKey EnumKey => Enum.Parse<TranslationKey>(Key);
    
    private readonly string[] _parameters = [];

    public TranslatableString(TranslationKey key)
    {
        _key = key.ToString();

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
    
    public TranslatableString(string key) : this(Enum.Parse<TranslationKey>(key)) { }
    
    public TranslatableString(TranslationKey key, params string[]? parameters) : this(key)
    {
        _parameters = parameters ?? [];
    }

    public string Value => _parameters == null || _parameters.Length == 0
        ? Translation.Get(EnumKey)
        : Translation.Get(EnumKey, _parameters);

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