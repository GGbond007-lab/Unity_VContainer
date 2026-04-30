using UnityEngine;
using System;

public class TestNewActionMsgHandler : IActionMsgHandler
{
    public string ActionName => "TestNewAction";

    private readonly Func<Type, IBaseAction> _actionFactory;
    private readonly ActionStack _actionStack;

    public TestNewActionMsgHandler(
        Func<Type, IBaseAction> actionFactory,
        ActionStack actionStack)
    {
        _actionFactory = actionFactory;
        _actionStack = actionStack;
    }

    public void Handle(string funcName, object data)
    {
        var currentEvent = _actionStack.GetCurrentAction();

        if (currentEvent != null && currentEvent is TestNewAction)
        {
            currentEvent.OnExecute(funcName, data);
            return;
        }

        var created = _actionFactory(typeof(TestNewAction));
        if (created == null)
        {
            Debug.LogError("事件工厂返回 null，无法创建 TestNewAction");
            return;
        }

        var newEvent = created as TestNewAction;
        if (newEvent == null)
        {
            Debug.LogError($"事件工厂创建的实例不能转换为 TestNewAction，实际类型：{created.GetType().Name}");
            return;
        }

        if (!string.IsNullOrEmpty(funcName))
        {
            newEvent.OnExecute(funcName, data);
        }
    }
}
