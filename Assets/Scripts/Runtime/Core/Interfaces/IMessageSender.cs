namespace UniVCon
{
    using Cysharp.Threading.Tasks;
    public interface IMessageSender {
        void SendActionMessage(string type, string actionName, string funcName, object data);
        void SendCurrentMessage(string type, string funcName, object data);
        void SendError(ActionExecutionResult result);
        UniTask SendActionMessageAsync(string type, string actionName, string funcName, object data);
        UniTask SendCurrentMessageAsync(string type, string funcName, object data);
        UniTask SendErrorAsync(ActionExecutionResult result);
    }
}
