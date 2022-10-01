using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace System.Reactive.Subjects
{
    public class SubjectWithValue<T> : ISubject<T>, ISubjectWithValue<T>
    {
        private readonly Subject<T> subject;

        public SubjectWithValue(T initialValue)
        {
            Value = initialValue;

            this.subject = new Subject<T>();
        }

        public T Value { get; private set; }

        public void OnCompleted()
        {
            this.subject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            this.subject.OnError(error);
        }

        public void OnNext(T value)
        {
            Value = value;
            this.subject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return this.subject.Subscribe(observer);
        }

        public void OnNextCurrent()
        {
            this.subject.OnNext(Value);
        }
    }
}
