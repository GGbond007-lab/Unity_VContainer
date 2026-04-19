using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VContainer;

public abstract class BaseEvent : IBaseEvent
{
    // 事件ID
    public string EventId { get; protected set; }

    #region 注入依赖
    // 标签控制器（子类可自由替换！）
    public IEventLabelController LabelCtrl { get; protected set; }

    // 注入全局标签管理器
    [Inject] protected ILabelManager LabelManager { get; private set; }

    // 方法映射表：存放 前端 funcName -> 可执行方法 的映射
    private readonly Dictionary<string, Action<object>> _methodMap = new();
    #endregion

    // 构造函数 → 保留
    protected BaseEvent()
    {
        EventId = Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 根据接收到的参数执行方法
    /// </summary>
    public virtual void OnExecute(string funcKey, object data)
    {
        if (_methodMap.TryGetValue(funcKey, out var action))
            action.Invoke(data);
    }

    public virtual void OnInitialize() { }

    public virtual void OnDestroy()
    {
        LabelCtrl?.ClearAll();
        LabelCtrl?.Destroy();
        LabelCtrl = null;
    }

    #region 根据配置绑定方法（使用 UnityEvent 可视化绑定，完美版）
    private EventConfigSO _config;

    [Inject]
    private void InitializeAfterInject()
    {
        RegisterLabelController();
    }

    protected virtual void RegisterLabelController()
    {
        LabelCtrl = null;
    }

    public void SetConfig(EventConfigSO config)
    {
        _config = config;
        BuildMethodBindingsFromConfig();
    }

    private void BuildMethodBindingsFromConfig()
    {
        if (_config == null) return;
        _methodMap.Clear();

        var type = GetType();

        // 处理普通方法绑定，支持无参方法和单 object 参数方法
        foreach (var binding in _config.methodBinds)
        {
            if (binding == null || !binding.enable || string.IsNullOrEmpty(binding.webFuncName) || string.IsNullOrEmpty(binding.unityFuncName))
                continue;

            var mapKey = binding.webFuncName;
            var method = type.GetMethod(binding.unityFuncName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                Debug.LogError($"找不到方法：{binding.unityFuncName} 在 {type.Name}");
                continue;
            }

            var pars = method.GetParameters();
            Action<object> action = null;
            if (pars.Length == 0)
            {
                action = data => method.Invoke(this, null);
            }
            else if (pars.Length == 1 && pars[0].ParameterType == typeof(object))
            {
                action = data => method.Invoke(this, new[] { data });
            }
            else
            {
                Debug.LogError($"方法签名不匹配（需要 void Method() 或 void Method(object)）：{binding.unityFuncName} 在 {type.Name}");
                continue;
            }

            // 包裹回调执行：如果启用了回调，会在方法执行完成后调用 callBackFuncName
            if (binding.callBackEnable && !string.IsNullOrEmpty(binding.callBackFuncName))
            {
                var callbackName = binding.callBackFuncName;
                var original = action;
                action = data =>
                {
                    original.Invoke(data);
                    // 调用回调
                    var cbMethod = type.GetMethod(callbackName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (cbMethod == null)
                    {
                        Debug.LogError($"找不到回调方法：{callbackName} 在 {type.Name}");
                        return;
                    }

                    var cbPars = cbMethod.GetParameters();
                    if (cbPars.Length == 0)
                    {
                        cbMethod.Invoke(this, null);
                    }
                    else if (cbPars.Length == 1 && cbPars[0].ParameterType == typeof(object))
                    {
                        cbMethod.Invoke(this, new[] { data });
                    }
                    else
                    {
                        Debug.LogError($"回调方法签名不匹配（需要 void Method() 或 void Method(object)）：{callbackName} 在 {type.Name}");
                    }
                };
            }

            _methodMap[mapKey] = action;
        }
    }
    #endregion
}