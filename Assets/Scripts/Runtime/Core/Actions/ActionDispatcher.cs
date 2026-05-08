namespace UniVCon
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    public sealed class ActionDispatcher {
        private readonly Func<Type, object[], IBaseAction> _actionFactory;
        private readonly ActionStack _actionStack;
        private readonly IActionConfigProvider _configProvider;
        public ActionDispatcher( Func<Type, object[], IBaseAction> actionFactory, ActionStack actionStack, IActionConfigProvider configProvider) {
            _actionFactory = actionFactory;
            _actionStack = actionStack;
            _configProvider = configProvider;
        }
        public async UniTask<ActionExecutionResult> Dispatch(string actionName, string funcName, object data) {
            await UniTask.SwitchToMainThread();
            if (string.IsNullOrEmpty(actionName)) {
                return await DispatchToCurrent(funcName, data);
            }
            var config = _configProvider.GetConfig(actionName);
            if (config == null) {
                return ActionExecutionResult.Fail( ActionErrorCode.ActionNotFound, $"Action '{actionName}' is not configured.", actionName, funcName);
            }
            var actionType = ActionRegistry.GetActionType(config.targetActionClassName);
            if (actionType == null) {
                return ActionExecutionResult.Fail( ActionErrorCode.ActionNotFound, $"Action type '{config.targetActionClassName}' is not registered.", actionName, funcName);
            }
            var current = _actionStack.GetCurrentAction();
            IBaseAction action;
            if (current != null && current.GetType() == actionType) {
                action = current;
            }
            else {
                try {
                    action = _actionFactory(actionType, null);
                }
                catch (Exception e) {
                    Debug.LogError($"[ActionDispatcher] Create action {actionType.Name} failed: {e}");
                    return ActionExecutionResult.Fail( ActionErrorCode.ExecutionFailed, e.Message, actionName, funcName);
                }
                var pushResult = _actionStack.TryPush(action);
                if (!pushResult.Success && pushResult.ErrorCode != ActionErrorCode.DuplicateAction) return pushResult;
                action = _actionStack.GetCurrentAction();
            }
            if (string.IsNullOrEmpty(funcName)) return ActionExecutionResult.Ok(actionName, funcName);
            return await action.OnExecute(funcName, data);
        }
        private async UniTask<ActionExecutionResult> DispatchToCurrent(string funcName, object data) {
            await UniTask.SwitchToMainThread();
            var currentAction = _actionStack.GetCurrentAction();
            if (currentAction == null) {
                return ActionExecutionResult.Fail( ActionErrorCode.ActionNotFound, "There is no current action on the stack.", null, funcName);
            }
            return await currentAction.OnExecute(funcName, data);
        }
    }
}
