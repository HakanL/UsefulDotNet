namespace System.Reactive.Linq
{
    //
    // Summary:
    //     Defines a provider for push-based notification.
    //
    // Type parameters:
    //   T:
    //     The object that provides notification information.
    public interface IObservableWithValue<out T> : IObservable<T>
    {
        void OnNextCurrent();

        T Value { get; }
    }
}
