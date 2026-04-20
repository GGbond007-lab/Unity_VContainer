using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VContainer;

public abstract class BaseEvent : IBaseEvent
{
    public string EventId { get; protected set; }

    [Inject] protected IEventBus EventBus { get; private set; }
    [Inject] protected ILabelManager LabelManager { get; private set; }

    protected EventConfigSO Config { get; private set; }

    public IEventLabelController LabelCtrl { get; protected set; }

    private readonly Dictionary<string, (string realMethodName, Action<object> action)> _methodMap = new();
    private List<IDisposable> _subscribeDisposables = new List<IDisposable>();

    private bool _initialized;

    protected BaseEvent()
    {
        EventId = Guid.NewGuid().ToString("N");
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

    //public void InvokeMethod(string methodName, object data = null)
    //{
    //    Debug.Log($"【🟢 手动调用】方法: {methodName}");

    //    var method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    //    if (method == null)
    //    {
    //        Debug.LogError($"【🔴 失败】找不到方法: {methodName}");
    //        return;
    //    }

    //    if (method.GetParameters().Length == 0)
    //        method.Invoke(this, null);
    //    else
    //        method.Invoke(this, new[] { data });

    //    var msg = new EventMethodExecutedMessage(this, methodName, data);
    //    Debug.Log($"【📤 发布消息】事件: {this.GetType().Name}  方法: {methodName}");
    //    EventBus.Publish(msg);
    //}

    [Inject]
    private void InitializeAfterInject()
    {
        if (_initialized) return;
        _initialized = true;

        Type selfType = GetType();

        // 🔥🔥🔥 关键：按 targetEventScript 绑定的脚本匹配配置
        Config = EventConfigProvider.GetConfigByTargetScript(selfType);

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
            if (string.IsNullOrEmpty(sub.targetEventClassName)
             || string.IsNullOrEmpty(sub.methodNameInTargetEvent)
             || string.IsNullOrEmpty(sub.localMethodName)) continue;

            Type targetEventType = FindEventType(sub.targetEventClassName);
            if (targetEventType == null) continue;

            MethodInfo localMethod = GetType().GetMethod(sub.localMethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (localMethod == null) continue;

            Debug.Log($"【✅ 订阅成功】监听: {targetEventType.Name}.{sub.methodNameInTargetEvent} → 执行: {sub.localMethodName}");

            var disposable = EventBus.Subscribe<EventMethodExecutedMessage>(msg =>
            {
                bool matchEvent = msg.Event.GetType() == targetEventType;
                bool matchMethod = msg.MethodName == sub.methodNameInTargetEvent;

                if (matchEvent && matchMethod)
                {
                    Debug.Log($"【🔥 触发】{targetEventType.Name}.{msg.MethodName} → 执行 {sub.localMethodName}");

                    // ==============================
                    // 🔥 🔥 🔥 核心修复：智能参数传递
                    // ==============================
                    var parameters = localMethod.GetParameters();

                    if (parameters.Length == 0)
                    {
                        // 无参数 → 不传
                        localMethod.Invoke(this, null);
                    }
                    else
                    {
                        var paramType = parameters[0].ParameterType;
                        object arg = null;

                        // 如果参数是事件类型 → 传事件本身
                        if (typeof(IBaseEvent).IsAssignableFrom(paramType))
                        {
                            arg = msg.Event;
                        }
                        // 否则 → 传数据
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
                if (t.Name == name && typeof(IBaseEvent).IsAssignableFrom(t))
                    return t;
        return null;
    }

    private void BuildMethodBindingsFromConfig()
    {
        if (Config == null) return;

        _methodMap.Clear();
        var type = GetType();

        foreach (var b in Config.methodBinds)
        {
            if (!b.enable || string.IsNullOrEmpty(b.webFuncName) || string.IsNullOrEmpty(b.unityFuncName)) continue;

            var method = type.GetMethod(b.unityFuncName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null) continue;

            Action<object> action = null;
            var ps = method.GetParameters();
            if (ps.Length == 0)
                action = d => method.Invoke(this, null);
            else if (ps.Length == 1 && ps[0].ParameterType == typeof(object))
                action = d => method.Invoke(this, new[] { d });
            else continue;

            if (!string.IsNullOrEmpty(b.callBackFuncName))
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

    // 废弃
    //public void SetConfig(EventConfigSO config) { }
    public void OnInitialize() { }
}

public class EventMethodExecutedMessage
{
    public IBaseEvent Event { get; }
    public string MethodName { get; }
    public object Data { get; }

    public EventMethodExecutedMessage(IBaseEvent evt, string methodName, object data)
    {
        Event = evt;
        MethodName = methodName;
        Data = data;
    }
}