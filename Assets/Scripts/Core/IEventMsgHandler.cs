public interface IEventMsgHandler
{
    string EventName { get; } // 前端传的 eventName 
    void Handle(string funcName, object data);
}