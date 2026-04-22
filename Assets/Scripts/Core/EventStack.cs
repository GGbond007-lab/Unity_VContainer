/// <summary>
/// 事件栈：入栈、出栈、获取当前/上一个事件
/// </summary>
public class EventStack
{
    private readonly System.Collections.Generic.Stack<IBaseEvent> _stack = new();
    private IBaseEvent _lastEvent;

    public void Push(IBaseEvent evt)
    {
        if (_stack.Count > 0)
            _lastEvent = _stack.Peek();

        _stack.Push(evt);
        evt.OnInitialize();
        UnityEngine.Debug.Log($"[事件栈] 入栈  → 总数：{_stack.Count}");
    }

    public IBaseEvent Pop()
    {
        if (_stack.Count == 0) return null;
        var evt = _stack.Pop();
        evt.OnDestroy();
        UnityEngine.Debug.Log($"[事件栈] 出栈  → 总数：{_stack.Count}");
        return evt;
    }
    // ✅ 加上这个方法，就能用 FindEvent<T> 了
    public T FindEvent<T>() where T : class, IBaseEvent
    {
        // 遍历栈中所有事件，找到匹配的类型
        foreach (var evt in _stack)
        {
            if (evt is T matchEvent)
            {
                return matchEvent;
            }
        }
        return null;
    }
    public IBaseEvent GetCurrentEvent() => _stack.Count > 0 ? _stack.Peek() : null;
    public IBaseEvent GetLastEvent() => _lastEvent;
}