using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YourAction3MsgHandler : IActionMsgHandler
{
    public string ActionName => "YourAction3";

    private readonly Func<Type, object[], IBaseAction> _actionFactory;
    private readonly ActionStack _actionStack;

    public YourAction3MsgHandler(
        Func<Type, object[], IBaseAction> actionFactory,
        ActionStack actionStack)
    {
        _actionFactory = actionFactory;
        _actionStack = actionStack;
    }

    public async UniTask Handle(string funcName, object data)
    {
        var currentAction = _actionStack.GetCurrentAction();

        if (currentAction is YourAction3)
        {
            await currentAction.OnExecute(funcName, data);
            return;
        }

        var created = _actionFactory(typeof(YourAction3), null);
        if (created == null)
        {
            Debug.LogError("Action factory returned null for YourAction3.");
            return;
        }

        if (created is not YourAction3 newAction)
        {
            Debug.LogError($"Action factory created {created.GetType().Name}, expected YourAction3.");
            return;
        }

        if (!string.IsNullOrEmpty(funcName))
        {
            await newAction.OnExecute(funcName, data);
        }
    }
}
