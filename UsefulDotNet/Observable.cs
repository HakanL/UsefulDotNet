using System;
using System.Reactive.Subjects;
using Haukcode.Reactive.Subjects;

namespace Haukcode.Reactive.Linq
{
    public static partial class Observable
    {
        /// <summary>
        /// Hides the identity of an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">An observable sequence whose identity to hide.</param>
        /// <returns>An observable sequence that hides the identity of the source sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        public static IObservableWithValue<TSource> AsObservableWithValue<TSource>(this ISubjectWithValue<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new ObservableWithValue<TSource>(source);
        }
    }
}
