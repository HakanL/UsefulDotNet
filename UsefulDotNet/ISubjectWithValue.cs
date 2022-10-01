using System.Reactive.Subjects;

namespace System.Reactive.Subjects
{
    public interface ISubjectWithValue<T> : ISubject<T>
    {
        /// <summary>
        /// Current value
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Call OnNext with the current value
        /// </summary>
        void OnNextCurrent();
    }
}
