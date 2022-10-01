using System.Reactive.Subjects;

namespace System.Reactive.Linq
{
    public class ObservableWithValue<T> : IObservableWithValue<T>
    {
        private readonly ISubjectWithValue<T> impl;

        public ObservableWithValue(ISubjectWithValue<T> impl)
        {
            this.impl = impl;
        }

        public void OnNextCurrent()
        {
            this.impl.OnNextCurrent();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return this.impl.Subscribe(observer);
        }

        public T Value => this.impl.Value;
    }
}