namespace UniVCon
{
    public enum ActionErrorCode {
        None, InvalidJson, InvalidMessageType, ActionNotFound, FunctionNotFound, InvalidPayload, ExecutionFailed, ActionCancelled, ActionDestroyed, ActionBusy, DuplicateAction, ConfigMissing
    }
    public sealed class ActionExecutionResult {
        public bool Success {
            get;
        }
        public ActionErrorCode ErrorCode {
            get;
        }
        public string Message {
            get;
        }
        public string ActionName {
            get;
        }
        public string FuncName {
            get;
        }
        public object Data {
            get;
        }
        private ActionExecutionResult( bool success, ActionErrorCode errorCode, string message, string actionName, string funcName, object data) {
            Success = success;
            ErrorCode = errorCode;
            Message = message;
            ActionName = actionName;
            FuncName = funcName;
            Data = data;
        }
        public static ActionExecutionResult Ok(string actionName = null, string funcName = null, object data = null) {
            return new ActionExecutionResult(true, ActionErrorCode.None, null, actionName, funcName, data);
        }
        public static ActionExecutionResult Fail( ActionErrorCode errorCode, string message, string actionName = null, string funcName = null, object data = null) {
            return new ActionExecutionResult(false, errorCode, message, actionName, funcName, data);
        }
    }
}
