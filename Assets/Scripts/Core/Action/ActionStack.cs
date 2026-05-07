/// <summary>
/// Action 栈：入栈、出栈、获取当前和上一个 Action
/// </summary>
public class ActionStack
{
    private readonly System.Collections.Generic.Stack<IBaseAction> _stack = new();
    private IBaseAction _lastAction;

    public void Push(IBaseAction action)
    {
        if (_stack.Count > 0)
            _lastAction = _stack.Peek();

        _stack.Push(action);
        action.OnInitialize();
        action.OnPushed();
        UnityEngine.Debug.Log($"[Action 栈] 入栈，当前总数：{_stack.Count}");
    }

    public IBaseAction Pop()
    {
        if (_stack.Count == 0) return null;
        var action = _stack.Pop();
        action.OnDestroy();
        UnityEngine.Debug.Log($"[Action 栈] 出栈，当前总数：{_stack.Count}");
        return action;
    }

    public T FindAction<T>() where T : class, IBaseAction
    {
        foreach (var action in _stack)
        {
            if (action is T matchAction)
            {
                return matchAction;
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
            UnityEngine.Debug.LogWarning($"[Action 栈] PopTo<{typeof(T).Name}> 失败：栈中未找到目标Action");
            return false;
        }
        return PopTo(target);
    }

    public bool PopTo(IBaseAction targetAction)
    {
        if (targetAction == null)
        {
            UnityEngine.Debug.LogWarning("[Action 栈] PopTo 失败：目标Action为空");
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
            UnityEngine.Debug.LogWarning("[Action 栈] PopTo 失败：目标Action不在栈中");
            return false;
        }

        int popCount = _stack.Count - targetIndex - 1;
        UnityEngine.Debug.Log($"[Action 栈] PopTo {targetAction.GetType().Name}，将弹出 {popCount} 个Action");

        for (int i = 0; i < popCount; i++)
        {
            Pop();
        }

        return true;
    }

    public void PopAll()
    {
        int count = _stack.Count;
        UnityEngine.Debug.Log($"[Action 栈] PopAll，共 {count} 个Action");
        while (_stack.Count > 0)
        {
            Pop();
        }
    }
}
