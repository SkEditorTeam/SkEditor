using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using R3;

namespace SkEditor.Utilities.Extensions;

public static class ObservableCollectionExtensions
{
    public static IDisposable BindTo<TSource, TTarget>(
        this ReadOnlyObservableCollection<TSource> source,
        ObservableCollection<TTarget> target,
        Func<TSource, TTarget> mapper,
        Func<TSource, bool>? filter = null,
        int maxItems = 10)
    {
        Sync();

        INotifyCollectionChanged incc = source;

        return Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
            h => (_, e) => h(e),
            h => incc.CollectionChanged += h,
            h => incc.CollectionChanged -= h
        ).Subscribe(_ => Sync());

        void Sync()
        {
            target.Clear();
            foreach (TSource item in source.Where(item => filter?.Invoke(item) ?? true).Take(maxItems))
            {
                target.Add(mapper(item));
            }
        }
    }
}