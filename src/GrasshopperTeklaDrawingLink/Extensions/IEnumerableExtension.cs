﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GTDrawingLink.Extensions
{
    public static class IEnumerableExtension
    {
        public static T ElementAtOrLast<T>(this IList<T> source, int index)
        {
            return source[Math.Min(index, source.Count - 1)];
        }

        public static IEnumerable<T> Safe<T>(this IEnumerable<T> source)
        {
            if (source == null)
                yield break;

            foreach (var item in source)
                yield return item;
        }

        public static bool HasItems<T>(this IEnumerable<T> source) => source != null && source.Any();
    }
}
