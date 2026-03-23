using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SkEditor.Utilities.Types;

public class SortedObservableCollection<T>(IComparer<T>? comparer = null) : ObservableCollection<T>
{
    private readonly IComparer<T> _comparer = comparer ?? Comparer<T>.Default;

    protected override void InsertItem(int index, T item)
    {
        int sortedIndex = FindSortedIndex(item);
        base.InsertItem(sortedIndex, item);
    }

    private int FindSortedIndex(T item)
    {
        int lo = 0, hi = Count;
        while (lo < hi)
        {
            int mid = (lo + hi) / 2;
            if (_comparer.Compare(Items[mid], item) <= 0)
                lo = mid + 1;
            else
                hi = mid;
        }

        return lo;
    }
}