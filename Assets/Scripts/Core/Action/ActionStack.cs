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
    public int Count => _stack.Count;
    public bool IsAtBottom(IBaseAction action) => _stack.Count > 0 && _stack.Peek() == action && _stack.Count == 1;

    public bool PopTo<T>() where T : class, IBaseAction
    {
        var target = FindAction<T>();
        if (target == null)
        {
            UnityEngine.Debug.LogWarning($"[事件栈] PopTo<{typeof(T).Name}> 失败：栈中未找到该Action");
            return false;
        }
        return PopTo(target);
    }

    public bool PopTo(IBaseAction targetAction)
    {
        if (targetAction == null)
        {
            UnityEngine.Debug.LogWarning("[事件栈] PopTo 失败：目标action为null");
            return false;
        }

        var tempList = new System.Collections.Generic.List<IBaseAction>(_stack);
        int targetIndex = -1;
        for (int i = 0; i < tempList.Count; i++)
        {
            if (tempList[i] == targetAction)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex < 0)
        {
            UnityEngine.Debug.LogWarning($"[事件栈] PopTo 失败：目标action不在栈中");
            return false;
        }

        int popCount = _stack.Count - targetIndex - 1;
        UnityEngine.Debug.Log($"[事件栈] PopTo {targetAction.GetType().Name}：将弹出 {popCount} 个action");

        for (int i = 0; i < popCount; i++)
        {
            Pop();
        }

        return true;
    }

    public void PopAll()
    {
        int count = _stack.Count;
        UnityEngine.Debug.Log($"[事件栈] PopAll：共 {count} 个action");
        while (_stack.Count > 0)
        {
            Pop();
        }
    }
}