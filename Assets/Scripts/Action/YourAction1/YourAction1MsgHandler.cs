using System;
using UnityEngine;

public class YourAction1MsgHandler : IActionMsgHandler
{
    public string ActionName => "你的Action1"; // 和SO里的 actionName 一致

    private readonly Func<Type, IBaseAction> _actionFactory;
    private readonly IActionBus _actionBus;
    private readonly ActionStack _actionStack;
    public YourAction1MsgHandler(
        Func<Type, IBaseAction> actionFactory,
        IActionBus actionBus,
        ActionStack actionStack)
    {
        _actionFactory = actionFactory;
        _actionBus = actionBus;
        _actionStack = actionStack;
    }

    public void Handle(string funcName, object data)
    {
        // 1. 获取事件栈里 当前正在运行的事件
        var currentEvent = _actionStack.GetCurrentAction();

        // 2. 判断：栈里已有事件 + 且和当前要创建的事件是【同一个类型】
        if (currentEvent != null && currentEvent is YourAction1)
        {
            // 🔥 不创建新事件！直接复用当前事件，只执行方法
            currentEvent.OnExecute(funcName, data);
            return;
        }

        var created = _actionFactory(typeof(YourAction1));
        if (created == null)
        {
            Debug.LogError("事件工厂返回 null，无法创建 YourEvent1");
            return;
        }

        var newEvent = created as YourAction1;
        if (newEvent == null)
        {
            Debug.LogError($"事件工厂创建的实例不能转换为 YourEvent1，实际类型：{created.GetType().Name}");
            return;
        }

        // 执行方法
        if (!string.IsNullOrEmpty(funcName))
        {
            newEvent.OnExecute(funcName, data);
        }
    }
}