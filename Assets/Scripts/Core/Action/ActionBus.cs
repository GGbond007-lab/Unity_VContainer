using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionBus : IActionBus
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly object _sync = new();

    public void Publish<T>(T message)
    {
        List<Delegate> snapshot;
        var type = typeof(T);

        lock (_sync)
        {
            if (!_subscribers.TryGetValue(type, out var callbacks) || callbacks.Count == 0)
                return;

            snapshot = new List<Delegate>(callbacks);
        }

        foreach (var del in snapshot)
        {
            try
            {
                if (del is Action<T> callback)
                    callback(message);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ActionBus] Subscriber for {type.Name} failed: {e}");
            }
        }
    }

    public IDisposable Subscribe<T>(Action<T> callback)
    {
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        var type = typeof(T);
        lock (_sync)
        {
            if (!_subscribers.TryGetValue(type, out var callbacks))
            {
                callbacks = new List<Delegate>();
                _subscribers[type] = callbacks;
            }

            if (!callbacks.Contains(callback))
                callbacks.Add(callback);
        }

        return new Subscription<T>(this, callback);
    }

    public void UnSubscribe<T>(Action<T> callback)
    {
        if (callback == null)
            return;

        var type = typeof(T);
        lock (_sync)
        {
            if (!_subscribers.TryGetValue(type, out var callbacks))
                return;

            callbacks.Remove(callback);
            if (callbacks.Count == 0)
                _subscribers.Remove(type);
        }
    }

    private sealed class Subscription<T> : IDisposable
    {
        private ActionBus _bus;
        private Action<T> _callback;

        public Subscription(ActionBus bus, Action<T> callback)
        {
            _bus = bus;
            _callback = callback;
        }

        public void Dispose()
        {
            var bus = _bus;
            var callback = _callback;
            if (bus == null || callback == null)
                return;

            _bus = null;
            _callback = null;
            bus.UnSubscribe(callback);
        }
    }
}
