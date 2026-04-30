using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class WebMsgHandlerManager
{
    private Dictionary<string, IActionMsgHandler> _handlerDic = new();
    private ActionStack _actionStack;
    // VContainer 自动注入所有 Handler
    public WebMsgHandlerManager(IEnumerable<IActionMsgHandler> handlers,ActionStack actionStack)
    {
        _actionStack = actionStack;
        var count = 0;
        foreach (var handler in handlers)
        {
            _handlerDic.Add(handler.ActionName, handler);
            UnityEngine.Debug.Log($"[WebMsgHandlerManager] Registered handler: {handler.ActionName} -> {handler.GetType().Name}");
            count++;
        }
        UnityEngine.Debug.Log($"[WebMsgHandlerManager] Total handlers registered: {count}");
    }


    //public void Receive(string json)
    //{
    //    WebMessage msg = JsonConvert.DeserializeObject<WebMessage>(json);
    //    string funcName = msg.funcName;
    //    object data = msg.data;
    //    var currentEvent = _eventStack.GetCurrentEvent();
    //    if (currentEvent is IBaseEvent exec)
    //    {
    //        exec.OnExecute(funcName, data);
    //    }
    //}
    public void ReceiveMessageFromWeb(string json)
    {
        try
        {
            // ==========================================================================
            // 第一步：尝试解析【带 eventName】的格式
            // ==========================================================================
            var msgEFD = JsonConvert.DeserializeObject<WebMessageEFD>(json);
            if (!string.IsNullOrEmpty(msgEFD.actionName))
            {
                // 有 actionName → 走的事件分发逻辑
                string actionName = msgEFD.actionName;
                string funcName = msgEFD.funcName;
                object data = msgEFD.data;

                Debug.Log($"✅ 接收[指定事件] action={actionName} func={funcName}");

                if (_handlerDic.TryGetValue(actionName, out var handler))
                {
                    handler.Handle(funcName, data);
                }
                else
                {
                    Debug.LogWarning($"未找到事件处理器：{actionName}");
                }
                return;
            }

            // ==========================================================================
            // 第二步：无 eventName → 解析【仅 funcName + data】格式
            // ==========================================================================
            var msg = JsonConvert.DeserializeObject<WebMessageFD>(json);
            string currentFunc = msg.funcName;
            object currentData = msg.data;

            Debug.Log($"✅ 接收[当前事件] func={currentFunc}");

            // 🔥 核心：没有 eventName → 自动在当前事件执行
            var currentEvent = _actionStack.GetCurrentAction();
            if (currentEvent is IBaseAction exec)
            {
                exec.OnExecute(currentFunc, currentData);
            }
            else
            {
                Debug.LogWarning("当前没有正在运行的事件！");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ JSON 解析失败：{e.Message}\n{json}");
        }
    }
}
