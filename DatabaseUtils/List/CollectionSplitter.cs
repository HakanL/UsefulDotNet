using System;
using System.Collections.Generic;
using System.Linq;

namespace Haukcode.DatabaseUtils
{
    public static class CollectionSplitter
    {
        /// <summary>
        /// Split list into X parts/batches
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="source">Source list</param>
        /// <param name="parts">Number of "buckets"/parts</param>
        /// <returns>List of list of source items</returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(IEnumerable<T> source, int parts)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / parts)
                .Select(x => x.Select(v => v.Value));
        }
    }
}
