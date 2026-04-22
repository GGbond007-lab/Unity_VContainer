using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class WebMsgHandlerManager
{
    private Dictionary<string, IEventMsgHandler> _handlerDic = new();
    private EventStack _eventStack;
    // VContainer 自动注入所有 Handler
    public WebMsgHandlerManager(IEnumerable<IEventMsgHandler> handlers,EventStack eventStack)
    {
        _eventStack = eventStack;
        var count = 0;
        foreach (var handler in handlers)
        {
            _handlerDic.Add(handler.EventName, handler);
            UnityEngine.Debug.Log($"[WebMsgHandlerManager] Registered handler: {handler.EventName} -> {handler.GetType().Name}");
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
    public void Receive(string json)
    {
        try
        {
            // ==========================================================================
            // 第一步：尝试解析【带 eventName】的格式
            // ==========================================================================
            var msgEFD = JsonConvert.DeserializeObject<WebMessageEFD>(json);
            if (!string.IsNullOrEmpty(msgEFD.eventName))
            {
                // 有 eventName → 走的事件分发逻辑
                string eventName = msgEFD.eventName;
                string funcName = msgEFD.funcName;
                object data = msgEFD.data;

                Debug.Log($"✅ 接收[指定事件] event={eventName} func={funcName}");

                if (_handlerDic.TryGetValue(eventName, out var handler))
                {
                    handler.Handle(funcName, data);
                }
                else
                {
                    Debug.LogWarning($"未找到事件处理器：{eventName}");
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
            var currentEvent = _eventStack.GetCurrentEvent();
            if (currentEvent is IBaseEvent exec)
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
[System.Serializable]
public class WebMessageEFD//定义前端格式的消息结构，包含事件名、方法名和数据
{
    public string eventName;
    public string funcName;
    public object data;
}
public class WebMessageFD
{
    public string funcName; // 定义前端格式的消息结构，包含方法名和数据，没有事件名
    public object data;
}