using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class WebMsgHandlerManager
{
    private readonly Dictionary<string, IActionMsgHandler> _handlerDic = new();
    private readonly ActionStack _actionStack;

    public WebMsgHandlerManager(IEnumerable<IActionMsgHandler> handlers, ActionStack actionStack)
    {
        _actionStack = actionStack;
        var count = 0;
        foreach (var handler in handlers)
        {
            _handlerDic.Add(handler.ActionName, handler);
            Debug.Log($"[WebMsgHandlerManager] Registered handler: {handler.ActionName} -> {handler.GetType().Name}");
            count++;
        }
        Debug.Log($"[WebMsgHandlerManager] Total handlers registered: {count}");
    }

    public async UniTask ReceiveMessageFromWeb(string json)
    {
        try
        {
            var msgEFD = JsonConvert.DeserializeObject<WebMessageEFD>(json);
            if (!string.IsNullOrEmpty(msgEFD.actionName))
            {
                string actionName = msgEFD.actionName;
                string funcName = msgEFD.funcName;
                object data = msgEFD.data;

                Debug.Log($"接收[指定Action] action={actionName} func={funcName}");

                if (_handlerDic.TryGetValue(actionName, out var handler))
                {
                    await handler.Handle(funcName, data);
                }
                else
                {
                    Debug.LogWarning($"未找到Action处理器：{actionName}");
                }
                return;
            }

            var msg = JsonConvert.DeserializeObject<WebMessageFD>(json);
            string currentFunc = msg.funcName;
            object currentData = msg.data;

            Debug.Log($"接收[当前Action] func={currentFunc}");

            var currentAction = _actionStack.GetCurrentAction();
            if (currentAction is IBaseAction exec)
            {
                await exec.OnExecute(currentFunc, currentData);
            }
            else
            {
                Debug.LogWarning("当前没有正在运行的Action！");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON 解析失败：{e.Message}\n{json}");
        }
    }
}
