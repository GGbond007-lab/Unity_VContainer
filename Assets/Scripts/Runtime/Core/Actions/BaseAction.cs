namespace UniVCon
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using VContainer;
    public abstract class BaseAction : IBaseAction {
        public string ActionId {
            get;
            protected set;
        }
        public bool IsDestroyed {
            get;
            private set;
        }
        public CancellationToken CancellationToken => _destroyCancellation.Token;
        [Inject] protected IActionBus ActionBus {
            get;
            private set;
        }
        [Inject] protected ILabelManager LabelManager {
            get;
            private set;
        }
        [Inject] protected IMessageSender MessageSender {
            get;
            private set;
        }
        [Inject] protected ActionStack ActionStack {
            get;
            private set;
        }
        [Inject] private WebDataConverter DataConverter {
            get;
            set;
        }
        [Inject] private IActionConfigProvider ConfigProvider {
            get;
            set;
        }
        protected ActionConfigSO Config {
            get;
            private set;
        }
        public IActionLabelController LabelCtrl {
            get;
            protected set;
        }
        protected Dictionary<string, ScriptableObject> SOContainer {
            get;
        }
        = new();
        private readonly Dictionary<string, MethodBinding> _methodMap = new();
        private readonly List<IDisposable> _subscribeDisposables = new();
        private readonly CancellationTokenSource _destroyCancellation = new();
        private bool _initialized;
        protected BaseAction() {
            ActionId = Guid.NewGuid().ToString("N");
        }
        public virtual async UniTask<ActionExecutionResult> OnExecute(string webFuncName, object data) {
            if (IsDestroyed) {
                return ActionExecutionResult.Fail( ActionErrorCode.ActionDestroyed, $"{GetType().Name} is already destroyed.", GetConfigActionName(), webFuncName);
            }
            if (CancellationToken.IsCancellationRequested) {
                return ActionExecutionResult.Fail( ActionErrorCode.ActionCancelled, $"{GetType().Name} is cancelled.", GetConfigActionName(), webFuncName);
            }
            if (!_methodMap.TryGetValue(webFuncName, out var binding)) {
                return ActionExecutionResult.Fail( ActionErrorCode.FunctionNotFound, $"Function '{webFuncName}' is not configured for {GetType().Name}.", GetConfigActionName(), webFuncName);
            }
            try {
                await binding.WebCallable(this, data).AttachExternalCancellation(CancellationToken);
                ActionBus.Publish(new ActionMethodExecutedMessage(this, binding.UnityMethodName, data));
                if (binding.Callback != null) {
                    await binding.Callback(this, data).AttachExternalCancellation(CancellationToken);
                }
                return ActionExecutionResult.Ok(GetConfigActionName(), webFuncName);
            }
            catch (OperationCanceledException) {
                return ActionExecutionResult.Fail( ActionErrorCode.ActionCancelled, $"{GetType().Name}.{binding.UnityMethodName} was cancelled.", GetConfigActionName(), webFuncName);
            }
            catch (PayloadConversionException e) {
                return ActionExecutionResult.Fail( ActionErrorCode.InvalidPayload, e.Message, GetConfigActionName(), webFuncName);
            }
            catch (Exception e) {
                Debug.LogError($"[ActionExecution] {GetType().Name}.{binding.UnityMethodName} failed: {e}");
                return ActionExecutionResult.Fail( ActionErrorCode.ExecutionFailed, e.Message, GetConfigActionName(), webFuncName);
            }
        }
        [Inject] private void InitializeAfterInject() {
            if (_initialized) return;
            _initialized = true;
            var selfType = GetType();
            Config = ConfigProvider.GetConfigByTargetAction(selfType);
            if (Config == null) {
                Debug.LogError($"[Action] {selfType.Name} has no ActionConfigSO.");
                return;
            }
            BuildMethodBindingsFromConfig();
            AutoSubscribeFromConfig();
            LoadSOsFromConfig();
            RegisterLabelController();
        }
        protected T ConvertData<T>(object data) {
            try {
                return DataConverter.ConvertData<T>(data);
            }
            catch (Exception e) {
                throw new PayloadConversionException($"Invalid payload for {typeof(T).Name}: {e.Message}", e);
            }
        }
        protected bool TryConvertData<T>(object data, out T result) {
            try {
                result = ConvertData<T>(data);
                return true;
            }
            catch (PayloadConversionException e) {
                Debug.LogError(e.Message);
                result = default;
                return false;
            }
        }
        protected T GetSO<T>() where T : ScriptableObject {
            return GetSO<T>(typeof(T).Name);
        }
        protected T GetSO<T>(string key) where T : ScriptableObject {
            return SOContainer.TryGetValue(key, out var so) ? so as T : null;
        }
        protected void LoadSO<T>() where T : ScriptableObject {
            var so = ConfigProvider.GetScriptableObjectByType(typeof(T)) as T;
            if (so != null) {
                SOContainer[typeof(T).Name] = so;
            }
            else {
                Debug.LogWarning($"[Action] {GetType().Name} could not load SO {typeof(T).Name}.");
            }
        }
        private void LoadSOsFromConfig() {
            if (Config?.requiredSOs == null) return;
            foreach (var soConfig in Config.requiredSOs) {
                if (soConfig.soReference == null || string.IsNullOrEmpty(soConfig.typeName)) continue;
                if (!SOContainer.ContainsKey(soConfig.typeName)) {
                    SOContainer[soConfig.typeName] = soConfig.soReference;
                }
            }
        }
        private void NotifyLetBackIfNeeded() {
            if (Config == null || !Config.isLetBack || ActionStack.IsAtBottom(this)) return;
            MessageSender?.SendActionMessage("message", "DefaultAction", "showBackButton", new {
            }
            );
        }
        private void AutoSubscribeFromConfig() {
            if (Config?.subscribeBinds == null) return;
            foreach (var sub in Config.subscribeBinds) {
                if (string.IsNullOrEmpty(sub.targetActionClassName) || string.IsNullOrEmpty(sub.methodNameInTargetAction) || string.IsNullOrEmpty(sub.localMethodName)) {
                    continue;
                }
                var targetActionType = ActionRegistry.GetActionType(sub.targetActionClassName);
                if (targetActionType == null) continue;
                if (!ActionRegistry.TryGetSubscribeMethod(GetType(), sub.localMethodName, out var localMethod)) continue;
                var disposable = ActionBus.Subscribe<ActionMethodExecutedMessage>(msg => {
                    if (IsDestroyed || msg.Action.GetType() != targetActionType || msg.MethodName != sub.methodNameInTargetAction) return;
                    localMethod(this, msg).Forget();
                }
                );
                _subscribeDisposables.Add(disposable);
            }
        }
        private void BuildMethodBindingsFromConfig() {
            if (Config == null) return;
            _methodMap.Clear();
            var type = GetType();
            foreach (var bind in Config.methodBinds) {
                if (!bind.enableWebFunc || string.IsNullOrEmpty(bind.webFuncName) || string.IsNullOrEmpty(bind.unityFuncName)) {
                    continue;
                }
                if (!ActionRegistry.TryGetWebCallableMethod(type, bind.unityFuncName, out var webCallable)) {
                    Debug.LogWarning($"[Action] {type.Name}.{bind.unityFuncName} is not in ActionRegistry.");
                    continue;
                }
                ActionRegistry.ActionMethodDelegate callback = null;
                if (bind.callBackEnable && !string.IsNullOrEmpty(bind.callBackFuncName)) {
                    ActionRegistry.TryGetCallbackMethod(type, bind.callBackFuncName, out callback);
                }
                _methodMap[bind.webFuncName] = new MethodBinding( bind.unityFuncName, webCallable, bind.callBackFuncName, callback);
            }
        }
        protected virtual void RegisterLabelController() {
        }
        public virtual void OnPushed() {
            NotifyLetBackIfNeeded();
        }
        public virtual void OnDestroy() {
            if (IsDestroyed) return;
            IsDestroyed = true;
            _destroyCancellation.Cancel();
            foreach (var d in _subscribeDisposables) {
                d?.Dispose();
            }
            _subscribeDisposables.Clear();
            if (LabelCtrl != null) {
                LabelCtrl.ClearAll();
                LabelCtrl.Destroy();
                LabelCtrl = null;
            }
            _destroyCancellation.Dispose();
        }
        public virtual void OnInitialize() {
        }
        public void Dispose() {
            OnDestroy();
        }
        private string GetConfigActionName() {
            return string.IsNullOrEmpty(Config?.actionName) ? GetType().Name : Config.actionName;
        }
        private sealed class MethodBinding {
            public string UnityMethodName {
                get;
            }
            public ActionRegistry.ActionMethodDelegate WebCallable {
                get;
            }
            public string CallbackMethodName {
                get;
            }
            public ActionRegistry.ActionMethodDelegate Callback {
                get;
            }
            public MethodBinding( string unityMethodName, ActionRegistry.ActionMethodDelegate webCallable, string callbackMethodName, ActionRegistry.ActionMethodDelegate callback) {
                UnityMethodName = unityMethodName;
                WebCallable = webCallable;
                CallbackMethodName = callbackMethodName;
                Callback = callback;
            }
        }
    }
    public class ActionMethodExecutedMessage {
        public IBaseAction Action {
            get;
        }
        public string MethodName {
            get;
        }
        public object Data {
            get;
        }
        public ActionMethodExecutedMessage(IBaseAction action, string methodName, object data) {
            Action = action;
            MethodName = methodName;
            Data = data;
        }
    }
    public sealed class PayloadConversionException : Exception {
        public PayloadConversionException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
