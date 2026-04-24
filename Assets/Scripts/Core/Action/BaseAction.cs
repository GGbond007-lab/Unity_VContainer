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

    protected ActionConfigSO Config { get; private set; }

    public IActionLabelController LabelCtrl { get; protected set; }

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
        RegisterLabelController();
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
    // ✅ ✅ ✅ 【核心修复】这里加入了 enableWebFunc 和 callBackEnable 的开关判断
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

    public void OnInitialize() { }
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