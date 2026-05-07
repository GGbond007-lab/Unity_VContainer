using System;
using Cysharp.Threading.Tasks;

public class YourAction2MsgHandler : IActionMsgHandler
{
    public string ActionName => "你的Action2";

    private readonly Func<Type, object[], IBaseAction> _actionFactory;

    public YourAction2MsgHandler(Func<Type, object[], IBaseAction> actionFactory)
    {
        _actionFactory = actionFactory;
    }

    public async UniTask Handle(string funcName, object data)
    {
        var action = _actionFactory(typeof(YourAction2), null) as YourAction2;
        if (action == null)
            return;

        if (!string.IsNullOrEmpty(funcName))
        {
            await action.OnExecute(funcName, data);
        }
    }
}
