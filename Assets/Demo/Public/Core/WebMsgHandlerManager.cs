using System.Collections.Generic;

public class WebMsgHandlerManager
{
    private Dictionary<string, IEventMsgHandler> _handlerDic = new();
    private EventStack _eventStack;
    // VContainer 自动注入所有 Handler
    public WebMsgHandlerManager(IEnumerable<IEventMsgHandler> handlers,EventStack eventStack)
    {
        _eventStack = eventStack;
        foreach (var handler in handlers)
            _handlerDic.Add(handler.EventName, handler);
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
        if (_handlerDic.TryGetValue(eventName, out var handler))
            handler.Handle(funcName, data);
    }
}