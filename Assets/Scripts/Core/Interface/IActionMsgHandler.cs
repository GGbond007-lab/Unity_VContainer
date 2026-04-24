public interface IActionMsgHandler
{
    string ActionName { get; } // 前端传的 eventName 
    void Handle(string funcName, object data);
}