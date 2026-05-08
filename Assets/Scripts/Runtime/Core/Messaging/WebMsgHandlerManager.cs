namespace UniVCon
{
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UnityEngine;
    public class WebMsgHandlerManager {
        public const string MessageType = "message";
        private readonly ActionDispatcher _dispatcher;
        private readonly IMessageSender _messageSender;
        public WebMsgHandlerManager(ActionDispatcher dispatcher, IMessageSender messageSender) {
            _dispatcher = dispatcher;
            _messageSender = messageSender;
        }
        public async UniTask<ActionExecutionResult> ReceiveMessageFromWeb(string json) {
            if (string.IsNullOrWhiteSpace(json)) {
                return SendError(ActionExecutionResult.Fail( ActionErrorCode.InvalidJson, "Message json is empty."));
            }
            WebMessageEnvelope envelope;
            try {
                envelope = JsonConvert.DeserializeObject<WebMessageEnvelope>(json);
            }
            catch (JsonException e) {
                return SendError(ActionExecutionResult.Fail( ActionErrorCode.InvalidJson, e.Message));
            }
            if (envelope == null) {
                return SendError(ActionExecutionResult.Fail( ActionErrorCode.InvalidJson, "Message json could not be parsed."));
            }
            if (!string.Equals(envelope.type, MessageType, System.StringComparison.Ordinal)) {
                return SendError(ActionExecutionResult.Fail( ActionErrorCode.InvalidMessageType, $"Unsupported message type '{envelope.type}'. Expected '{MessageType}'.", envelope.actionName, envelope.funcName));
            }
            ActionExecutionResult result;
            try {
                result = await _dispatcher.Dispatch(envelope.actionName, envelope.funcName, envelope.data);
            }
            catch (JsonException e) {
                result = ActionExecutionResult.Fail( ActionErrorCode.InvalidPayload, e.Message, envelope.actionName, envelope.funcName);
            }
            catch (System.Exception e) {
                Debug.LogError($"[WebMsgHandlerManager] Dispatch failed: {e}");
                result = ActionExecutionResult.Fail( ActionErrorCode.ExecutionFailed, e.Message, envelope.actionName, envelope.funcName);
            }
            if (!result.Success) _messageSender.SendError(result);
            return result;
        }
        private ActionExecutionResult SendError(ActionExecutionResult result) {
            _messageSender.SendError(result);
            return result;
        }
        private sealed class WebMessageEnvelope {
            public string type {
                get;
                set;
            }
            public string actionName {
                get;
                set;
            }
            public string funcName {
                get;
                set;
            }
            public JToken data {
                get;
                set;
            }
        }
    }
}
