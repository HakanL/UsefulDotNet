using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace System.Reactive.Subjects
{
    public class SubjectWithValue<T> : ISubject<T>, ISubjectWithValue<T>
    {
        private readonly Subject<T> subject;
        private readonly bool distinct;

        public SubjectWithValue(T initialValue, bool distinct = false)
        {
            Value = initialValue;
            this.distinct = distinct;

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
            if (!this.distinct || !Value.Equals(value))
            {
                Value = value;
                this.subject.OnNext(value);
            }
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
