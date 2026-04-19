using System.Collections.Generic;

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


    public void Receive(string funcName,object data)
    {
        var currentEvent = _eventStack.GetCurrentEvent();
        if (currentEvent is IBaseEvent exec)
        {
            exec.OnExecute(funcName, data);
        }
    }
    public void Receive(string eventName, string funcName, object data)
    {
        UnityEngine.Debug.Log($"[WebMsgHandlerManager] Receive eventName={eventName} funcName={funcName} data={(data!=null?data.GetType().Name:"null")}");
        if (_handlerDic.TryGetValue(eventName, out var handler))
        {
            UnityEngine.Debug.Log($"[WebMsgHandlerManager] Found handler for {eventName}: {handler.GetType().Name}");
            handler.Handle(funcName, data);
        }
        else
        {
            UnityEngine.Debug.LogWarning($"[WebMsgHandlerManager] No handler registered for eventName={eventName}");
        }
    }
}