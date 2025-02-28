using System;
using System.Collections.Generic;

namespace Haukcode.UsefulDotNet.Extensions
{
    public static partial class Extensions
    {
        /// <summary>
        /// Split list into X parts/batches
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="source">Source list</param>
        /// <param name="parts">Number of "buckets"/parts</param>
        /// <returns>List of list of source items</returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int parts)
        {
            return CollectionSplitter.Split(source, parts);
        }
    }
}
