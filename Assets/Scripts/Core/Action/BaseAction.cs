using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VContainer;

public abstract class BaseAction : IBaseAction
{
    public string ActionId { get; protected set; }

    [Inject] protected IActionBus EventBus { get; private set; }
    [Inject] protected ILabelManager LabelManager { get; private set; }
    [Inject] protected IMessageSender MessageSender { get; private set; }
    [Inject] protected ActionStack ActionStack { get; private set; }

    protected ActionConfigSO Config { get; private set; }

    public IActionLabelController LabelCtrl { get; protected set; }

    protected Dictionary<string, ScriptableObject> SOContainer { get; } = new();

    private readonly Dictionary<string, (string realMethodName, Action<object> action)> _methodMap = new();
    private List<IDisposable> _subscribeDisposables = new List<IDisposable>();

    private bool _initialized;

    protected BaseAction()
    {
        ActionId = Guid.NewGuid().ToString("N");
    }

    public virtual void OnExecute(string webFuncName, object data)
    {
        Debug.Log($"【🟢 调用】OnExecute: webFuncName = {webFuncName}");

        if (_methodMap.TryGetValue(webFuncName, out var entry))
        {
            Debug.Log($"【🟢 找到映射】执行本地方法: {entry.realMethodName}");
            entry.action.Invoke(data);

            var msg = new EventMethodExecutedMessage(this, entry.realMethodName, data);
            Debug.Log($"【📤 发布消息】事件: {this.GetType().Name}  方法: {entry.realMethodName}");
            EventBus.Publish(msg);
        }
        else
        {
            Debug.LogError($"【🔴 失败】找不到 webFuncName: {webFuncName}");
        }
    }

    [Inject]
    private void InitializeAfterInject()
    {
        if (_initialized) return;
        _initialized = true;

        Type selfType = GetType();

        Config = ActionConfigProvider.GetConfigByTargetScript(selfType);

        if (Config == null)
        {
            Debug.LogError($"【🔴 错误】{selfType.Name} 没有找到绑定的配置！请检查 SO 的 targetEventScript");
            return;
        }

        Debug.Log($"【✅ 成功】{selfType.Name} 找到自己的配置：{Config.name}");

        BuildMethodBindingsFromConfig();
        AutoSubscribeFromConfig();
        LoadSOsFromConfig();
        RegisterLabelController();
        NotifyLetBackIfNeeded();
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
            Debug.Log($"【✅ 加载SO】{GetType().Name} 加载了 {typeof(T).Name}: {so.name}");
        }
        else
        {
            Debug.LogWarning($"【⚠️ 警告】{GetType().Name} 尝试加载 {typeof(T).Name} 但未找到SO");
        }
    }

    private void LoadSOsFromConfig()
    {
        if (Config == null || Config.requiredSOs == null || Config.requiredSOs.Count == 0)
        {
            Debug.Log($"【⏭️ 跳过】{GetType().Name} 没有配置需要的ScriptableObject");
            return;
        }

        foreach (var soConfig in Config.requiredSOs)
        {
            if (soConfig.soReference == null)
            {
                Debug.LogWarning($"【⚠️ 警告】{GetType().Name} 配置了一个空的ScriptableObject引用（key: {soConfig.typeName}）");
                continue;
            }

            if (string.IsNullOrEmpty(soConfig.typeName))
            {
                Debug.LogWarning($"【⚠️ 警告】{GetType().Name} 配置的ScriptableObject没有设置字典key");
                continue;
            }

            string key = soConfig.typeName;
            if (!SOContainer.ContainsKey(key))
            {
                SOContainer[key] = soConfig.soReference;
                Debug.Log($"【✅ 自动加载SO】{GetType().Name} 加载了 key='{key}': {soConfig.soReference.name}");
            }
            else
            {
                Debug.LogWarning($"【⚠️ 警告】{GetType().Name} 重复配置了相同的key: '{key}'");
            }
        }
    }

    private void NotifyLetBackIfNeeded()
    {
        if (Config == null || !Config.isLetBack) return;

        if (ActionStack.IsAtBottom(this))
        {
            Debug.Log($"【⏭️ 跳过返回通知】{GetType().Name} 处于栈底，不显示返回按钮");
            return;
        }

        if (MessageSender == null)
        {
            Debug.LogWarning($"【⚠️ 警告】{GetType().Name} 需要发送返回通知但 MessageSender 为 null");
            return;
        }

        //MessageSender.SendCurrentMessage("action", "showBackButton", new { actionName = this.GetType().Name });
        MessageSender.SendActionMessage("message", "DefaultAction","showBackButton", new {});
        Debug.Log($"【📤 发送返回通知】{GetType().Name} 允许返回，已通知前端显示返回按钮");
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

            Type targetEventType = FindEventType(sub.targetActionClassName);
            if (targetEventType == null) continue;

            MethodInfo localMethod = GetType().GetMethod(sub.localMethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (localMethod == null) continue;

            Debug.Log($"【✅ 订阅成功】监听: {targetEventType.Name}.{sub.methodNameInTargetAction} → 执行: {sub.localMethodName}");

            var disposable = EventBus.Subscribe<EventMethodExecutedMessage>(msg =>
            {
                bool matchEvent = msg.Event.GetType() == targetEventType;
                bool matchMethod = msg.MethodName == sub.methodNameInTargetAction;

                if (matchEvent && matchMethod)
                {
                    Debug.Log($"【🔥 触发】{targetEventType.Name}.{msg.MethodName} → 执行 {sub.localMethodName}");

                    var parameters = localMethod.GetParameters();

                    if (parameters.Length == 0)
                    {
                        localMethod.Invoke(this, null);
                    }
                    else
                    {
                        var paramType = parameters[0].ParameterType;
                        object arg = null;

                        if (typeof(IBaseAction).IsAssignableFrom(paramType))
                        {
                            arg = msg.Event;
                        }
                        else
                        {
                            arg = msg.Data;
                        }

                        try
                        {
                            localMethod.Invoke(this, new[] { arg });
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"【调用失败】{sub.localMethodName} 参数不匹配\n期望：{paramType}\n传入：{arg?.GetType()}\n{e}");
                        }
                    }
                }
            });

            _subscribeDisposables.Add(disposable);
        }
    }

    private Type FindEventType(string name)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var t in asm.GetTypes())
                if (t.Name == name && typeof(IBaseAction).IsAssignableFrom(t))
                    return t;
        return null;
    }

    // ==============================================================================================
    //这里加入了 enableWebFunc 和 callBackEnable 的开关判断
    // ==============================================================================================
    private void BuildMethodBindingsFromConfig()
    {
        if (Config == null) return;

        _methodMap.Clear();
        var type = GetType();

        foreach (var b in Config.methodBinds)
        {
            // --------------------------
            // 🔥 修复 1：enableWebFunc = false 直接跳过，不绑定
            // --------------------------
            if (!b.enableWebFunc)
            {
                Debug.Log($"【⏹️ 跳过】{b.webFuncName} 已被 enableWebFunc 关闭");
                continue;
            }

            // 必须填写的字段
            if (string.IsNullOrEmpty(b.webFuncName) || string.IsNullOrEmpty(b.unityFuncName))
                continue;

            var method = type.GetMethod(b.unityFuncName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null) continue;

            Action<object> action = null;
            var ps = method.GetParameters();
            if (ps.Length == 0)
                action = d => method.Invoke(this, null);
            else if (ps.Length == 1 && ps[0].ParameterType == typeof(object))
                action = d => method.Invoke(this, new[] { d });
            else continue;

            // --------------------------
            // 🔥 修复 2：callBackEnable = false 不绑定回调
            // --------------------------
            if (b.callBackEnable && !string.IsNullOrEmpty(b.callBackFuncName))
            {
                var cbName = b.callBackFuncName;
                var origin = action;
                action = d =>
                {
                    origin.Invoke(d);
                    var cb = type.GetMethod(cbName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (cb == null) return;
                    if (cb.GetParameters().Length == 0) cb.Invoke(this, null);
                    else if (cb.GetParameters().Length == 1) cb.Invoke(this, new[] { d });
                };
            }

            _methodMap[b.webFuncName] = (b.unityFuncName, action);
            Debug.Log($"【✅ 方法绑定】{b.webFuncName} → {b.unityFuncName}");
        }
    }

    protected virtual void RegisterLabelController() { }

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
}

public class EventMethodExecutedMessage
{
    public IBaseAction Event { get; }
    public string MethodName { get; }
    public object Data { get; }

    public EventMethodExecutedMessage(IBaseAction evt, string methodName, object data)
    {
        Event = evt;
        MethodName = methodName;
        Data = data;
    }
}