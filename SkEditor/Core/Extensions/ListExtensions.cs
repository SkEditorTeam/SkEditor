using System.Collections.Generic;

namespace SkEditor.Utilities.Extensions;

public static class ListExtensions
{
    public static int BinarySearch<T>(this IReadOnlyList<T> list, T value, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        int lo = 0;
        int hi = list.Count - 1;

        while (lo <= hi)
        {
            int i = lo + ((hi - lo) >> 1);
            int c = comparer.Compare(list[i], value);

            switch (c)
            {
                case 0:
                    return i;
                case < 0:
                    lo = i + 1;
                    break;
                default:
                    hi = i - 1;
                    break;
            }
        }

        return ~lo;
    }
}