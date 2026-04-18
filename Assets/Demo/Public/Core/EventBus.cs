public class EventBus : IEventBus
{
    private readonly System.Collections.Generic.Dictionary<System.Type, System.Delegate> _dic = new();

    public void Publish<T>(T evt) where T : IBaseEvent
    {
        System.Type type = typeof(T);
        if (_dic.TryGetValue(type, out var del))
            (del as System.Action<T>)?.Invoke(evt);
    }

    public void Subscribe<T>(System.Action<T> callback) where T : IBaseEvent
    {
        System.Type type = typeof(T);
        if (!_dic.ContainsKey(type)) _dic[type] = callback;
        else _dic[type] = System.Delegate.Combine(_dic[type], callback);
    }

    public void UnSubscribe<T>(System.Action<T> callback) where T : IBaseEvent
    {
        System.Type type = typeof(T);
        if (_dic.ContainsKey(type)) _dic[type] = System.Delegate.Remove(_dic[type], callback);
    }
}