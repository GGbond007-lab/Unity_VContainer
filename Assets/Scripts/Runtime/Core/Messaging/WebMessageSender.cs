namespace UniVCon
{
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using UnityEngine;
    public class WebMessageSender : IMessageSender {
        private readonly IMessageTransport _transport;
        public WebMessageSender(IMessageTransport transport) {
            _transport = transport;
        }
        public void SendActionMessage(string type, string actionName, string funcName, object data) {
            SendActionMessageAsync(type, actionName, funcName, data).Forget();
        }
        public UniTask SendActionMessageAsync(string type, string actionName, string funcName, object data) {
            var msg = new WebMessageEFD {
                type = type, actionName = actionName, funcName = funcName, data = data
            };
            var json = JsonConvert.SerializeObject(msg, Formatting.Indented);
            Debug.Log($"<color=green>[SEND]</color> Action: {actionName} | {funcName}\n{json}");
            return _transport.SendAsync(json);
        }
        public void SendCurrentMessage(string type, string funcName, object data) {
            SendCurrentMessageAsync(type, funcName, data).Forget();
        }
        public UniTask SendCurrentMessageAsync(string type, string funcName, object data) {
            var msg = new WebMessageFD {
                type = type, funcName = funcName, data = data
            };
            var json = JsonConvert.SerializeObject(msg, Formatting.Indented);
            Debug.Log($"<color=green>[SEND]</color> Current Action: {funcName}\n{json}");
            return _transport.SendAsync(json);
        }
        public void SendError(ActionExecutionResult result) {
            SendErrorAsync(result).Forget();
        }
        public UniTask SendErrorAsync(ActionExecutionResult result) {
            if (result == null || result.Success) return UniTask.CompletedTask;
            var response = WebErrorResponse.FromResult(result);
            var json = JsonConvert.SerializeObject(response, Formatting.Indented);
            Debug.LogError($"<color=red>[SEND ERROR]</color> {response.code}: {response.message}\n{json}");
            return _transport.SendAsync(json);
        }
    }
}
