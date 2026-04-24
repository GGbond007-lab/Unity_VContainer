using System;

public interface IActionBus
{
    void Publish<T>(T message);
    IDisposable Subscribe<T>(Action<T> callback);
    void UnSubscribe<T>(Action<T> callback);
}
