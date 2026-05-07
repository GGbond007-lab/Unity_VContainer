using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public abstract class BaseAction : IBaseAction
{
    public string ActionId { get; protected set; }

    [Inject] protected IActionBus ActionBus { get; private set; }
    [Inject] protected ILabelManager LabelManager { get; private set; }
    [Inject] protected IMessageSender MessageSender { get; private set; }
    [Inject] protected ActionStack ActionStack { get; private set; }
    [Inject] private WebDataConverter DataConverter { get; set; }

    protected ActionConfigSO Config { get; private set; }

    public IActionLabelController LabelCtrl { get; protected set; }

    protected Dictionary<string, ScriptableObject> SOContainer { get; } = new();

    private readonly Dictionary<string, MethodBinding> _methodMap = new();
    private readonly List<IDisposable> _subscribeDisposables = new();

    private bool _initialized;

    protected BaseAction()
    {
        ActionId = Guid.NewGuid().ToString("N");
    }

    public virtual async UniTask OnExecute(string webFuncName, object data)
    {
        Debug.Log($"【调用】OnExecute: webFuncName = {webFuncName}");

        if (!_methodMap.TryGetValue(webFuncName, out var binding))
        {
            Debug.LogError($"【失败】找不到 webFuncName: {webFuncName}");
            return;
        }

        Debug.Log($"【找到映射】执行本地方法: {binding.UnityMethodName}");
        await binding.WebCallable(this, data);

        var msg = new ActionMethodExecutedMessage(this, binding.UnityMethodName, data);
        Debug.Log($"【发布消息】Action: {GetType().Name} 方法: {binding.UnityMethodName}");
        ActionBus.Publish(msg);

        if (binding.Callback != null)
        {
            Debug.Log($"【执行回调】{GetType().Name}.{binding.CallbackMethodName}");
            await binding.Callback(this, data);
        }
    }

    [Inject]
    private void InitializeAfterInject()
    {
        if (_initialized) return;
        _initialized = true;

        Type selfType = GetType();

        Config = ActionConfigProvider.GetConfigByTargetAction(selfType);

        if (Config == null)
        {
            Debug.LogError($"【错误】{selfType.Name} 没有找到绑定的配置！请检查 SO 的 targetActionScript");
            return;
        }

        Debug.Log($"【成功】{selfType.Name} 找到自己的配置：{Config.name}");

        BuildMethodBindingsFromConfig();
        AutoSubscribeFromConfig();
        LoadSOsFromConfig();
        RegisterLabelController();
    }

    protected T ConvertData<T>(object data)
    {
        return DataConverter.ConvertData<T>(data);
    }

    protected bool TryConvertData<T>(object data, out T result)
    {
        return DataConverter.TryConvertData(data, out result);
    }

    protected T GetSO<T>() where T : ScriptableObject
    {
        var key = typeof(T).Name;
        if (SOContainer.TryGetValue(key, out var so))
        {
            return so as T;
        }
        return null;
    }

    protected T GetSO<T>(string key) where T : ScriptableObject
    {
        if (SOContainer.TryGetValue(key, out var so))
        {
            return so as T;
        }
        return null;
    }

    protected void LoadSO<T>() where T : ScriptableObject
    {
        var so = ActionConfigProvider.GetScriptableObjectByType(typeof(T)) as T;
        if (so != null)
        {
            var key = typeof(T).Name;
            SOContainer[key] = so;
            Debug.Log($"【加载SO】{GetType().Name} 加载了 {typeof(T).Name}: {so.name}");
        }
        else
        {
            Debug.LogWarning($"【警告】{GetType().Name} 尝试加载 {typeof(T).Name} 但未找到SO");
        }
    }

    private void LoadSOsFromConfig()
    {
        if (Config == null || Config.requiredSOs == null || Config.requiredSOs.Count == 0)
        {
            Debug.Log($"【跳过】{GetType().Name} 没有配置需要的ScriptableObject");
            return;
        }

        foreach (var soConfig in Config.requiredSOs)
        {
            if (soConfig.soReference == null)
            {
                Debug.LogWarning($"【警告】{GetType().Name} 配置了一个空的ScriptableObject引用（key: {soConfig.typeName}）");
                continue;
            }

            if (string.IsNullOrEmpty(soConfig.typeName))
            {
                Debug.LogWarning($"【警告】{GetType().Name} 配置的ScriptableObject没有设置字典key");
                continue;
            }

            string key = soConfig.typeName;
            if (!SOContainer.ContainsKey(key))
            {
                SOContainer[key] = soConfig.soReference;
                Debug.Log($"【自动加载SO】{GetType().Name} 加载了 key='{key}': {soConfig.soReference.name}");
            }
            else
            {
                Debug.LogWarning($"【警告】{GetType().Name} 重复配置了相同的key: '{key}'");
            }
        }
    }

    private void NotifyLetBackIfNeeded()
    {
        if (Config == null || !Config.isLetBack) return;

        if (ActionStack.IsAtBottom(this))
        {
            Debug.Log($"【跳过返回通知】{GetType().Name} 处于栈底，不显示返回按钮");
            return;
        }

        if (MessageSender == null)
        {
            Debug.LogWarning($"【警告】{GetType().Name} 需要发送返回通知但 MessageSender 为 null");
            return;
        }

        MessageSender.SendActionMessage("message", "DefaultAction", "showBackButton", new { });
        Debug.Log($"【发送返回通知】{GetType().Name} 允许返回，已通知前端显示返回按钮");
    }

    private void AutoSubscribeFromConfig()
    {
        if (Config == null) return;
        if (Config.subscribeBinds == null || Config.subscribeBinds.Count == 0) return;

        foreach (var sub in Config.subscribeBinds)
        {
            if (string.IsNullOrEmpty(sub.targetActionClassName)
             || string.IsNullOrEmpty(sub.methodNameInTargetAction)
             || string.IsNullOrEmpty(sub.localMethodName)) continue;

            Type targetActionType = ActionRegistry.GetActionType(sub.targetActionClassName);
            if (targetActionType == null)
            {
                Debug.LogWarning($"【跳过订阅】未在 {nameof(ActionRegistry)} 中找到目标Action：{sub.targetActionClassName}");
                continue;
            }

            if (!ActionRegistry.TryGetSubscribeMethod(GetType(), sub.localMethodName, out var localMethod))
            {
                Debug.LogWarning($"【跳过订阅】{GetType().Name}.{sub.localMethodName} 未加入 {nameof(ActionRegistry)} 的 SubscribeMethods");
                continue;
            }

            Debug.Log($"【订阅成功】监听: {targetActionType.Name}.{sub.methodNameInTargetAction} -> 执行: {sub.localMethodName}");

            var disposable = ActionBus.Subscribe<ActionMethodExecutedMessage>(msg =>
            {
                bool matchAction = msg.Action.GetType() == targetActionType;
                bool matchMethod = msg.MethodName == sub.methodNameInTargetAction;

                if (matchAction && matchMethod)
                {
                    Debug.Log($"【触发】{targetActionType.Name}.{msg.MethodName} -> 执行 {sub.localMethodName}");
                    localMethod(this, msg).Forget();
                }
            });

            _subscribeDisposables.Add(disposable);
        }
    }

    private void BuildMethodBindingsFromConfig()
    {
        if (Config == null) return;

        _methodMap.Clear();
        var type = GetType();

        foreach (var bind in Config.methodBinds)
        {
            if (!bind.enableWebFunc)
            {
                Debug.Log($"【跳过】{bind.webFuncName} 已被 enableWebFunc 关闭");
                continue;
            }

            if (string.IsNullOrEmpty(bind.webFuncName) || string.IsNullOrEmpty(bind.unityFuncName))
                continue;

            if (!ActionRegistry.TryGetWebCallableMethod(type, bind.unityFuncName, out var webCallable))
            {
                Debug.LogWarning($"【跳过方法绑定】{type.Name}.{bind.unityFuncName} 未加入 {nameof(ActionRegistry)} 的 WebCallableMethods");
                continue;
            }

            ActionRegistry.ActionMethodDelegate callback = null;
            if (bind.callBackEnable && !string.IsNullOrEmpty(bind.callBackFuncName))
            {
                if (!ActionRegistry.TryGetCallbackMethod(type, bind.callBackFuncName, out callback))
                {
                    Debug.LogWarning($"【跳过回调绑定】{type.Name}.{bind.callBackFuncName} 未加入 {nameof(ActionRegistry)} 的 CallbackMethods");
                }
            }

            _methodMap[bind.webFuncName] = new MethodBinding(
                bind.unityFuncName,
                webCallable,
                bind.callBackFuncName,
                callback
            );

            Debug.Log($"【方法绑定】{bind.webFuncName} -> {bind.unityFuncName}");
        }
    }

    protected virtual void RegisterLabelController() { }

    public virtual void OnPushed()
    {
        NotifyLetBackIfNeeded();
    }

    public virtual void OnDestroy()
    {
        foreach (var d in _subscribeDisposables) d?.Dispose();
        _subscribeDisposables.Clear();

        if (LabelCtrl != null)
        {
            LabelCtrl.ClearAll();
            LabelCtrl.Destroy();
            LabelCtrl = null;
        }
    }

    public virtual void OnInitialize() { }

    private sealed class MethodBinding
    {
        public string UnityMethodName { get; }
        public ActionRegistry.ActionMethodDelegate WebCallable { get; }
        public string CallbackMethodName { get; }
        public ActionRegistry.ActionMethodDelegate Callback { get; }

        public MethodBinding(
            string unityMethodName,
            ActionRegistry.ActionMethodDelegate webCallable,
            string callbackMethodName,
            ActionRegistry.ActionMethodDelegate callback)
        {
            UnityMethodName = unityMethodName;
            WebCallable = webCallable;
            CallbackMethodName = callbackMethodName;
            Callback = callback;
        }
    }
}

public class ActionMethodExecutedMessage
{
    public IBaseAction Action { get; }
    public string MethodName { get; }
    public object Data { get; }

    public ActionMethodExecutedMessage(IBaseAction action, string methodName, object data)
    {
        Action = action;
        MethodName = methodName;
        Data = data;
    }
}
