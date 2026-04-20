using System;
using System.Collections.Generic;

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, Delegate> _dic = new();

    public void Publish<T>(T message)
    {
        Type type = typeof(T);
        if (_dic.TryGetValue(type, out var del))
            (del as Action<T>)?.Invoke(message);
    }

    public IDisposable Subscribe<T>(Action<T> callback)
    {
        Type type = typeof(T);
        if (!_dic.ContainsKey(type))
            _dic[type] = callback;
        else
            _dic[type] = Delegate.Combine(_dic[type], callback);

        return new UnsubscribeDelegate<T>(this, callback);
    }

    public void UnSubscribe<T>(Action<T> callback)
    {
        Type type = typeof(T);
        if (_dic.ContainsKey(type))
            _dic[type] = Delegate.Remove(_dic[type], callback);
    }
}

// 自动取消订阅
public class UnsubscribeDelegate<T> : IDisposable
{
    private IEventBus _bus;
    private Action<T> _callback;

    public UnsubscribeDelegate(IEventBus bus, Action<T> callback)
    {
        _bus = bus;
        _callback = callback;
    }

    public void Dispose()
    {
        _bus?.UnSubscribe(_callback);
    }
}