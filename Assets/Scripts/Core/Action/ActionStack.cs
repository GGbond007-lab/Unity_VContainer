/// <summary>
/// 事件栈：入栈、出栈、获取当前/上一个事件
/// </summary>
public class ActionStack
{
    private readonly System.Collections.Generic.Stack<IBaseAction> _stack = new();
    private IBaseAction _lastAction;

    public void Push(IBaseAction evt)
    {
        if (_stack.Count > 0)
            _lastAction = _stack.Peek();

        _stack.Push(evt);
        evt.OnInitialize();
        UnityEngine.Debug.Log($"[事件栈] 入栈  → 总数：{_stack.Count}");
    }

    public IBaseAction Pop()
    {
        if (_stack.Count == 0) return null;
        var evt = _stack.Pop();
        evt.OnDestroy();
        UnityEngine.Debug.Log($"[事件栈] 出栈  → 总数：{_stack.Count}");
        return evt;
    }
    // ✅ 加上这个方法，就能用 FindEvent<T> 了
    public T FindAction<T>() where T : class, IBaseAction
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
    public IBaseAction GetCurrentAction() => _stack.Count > 0 ? _stack.Peek() : null;
    public IBaseAction GetLastAction() => _lastAction;
}