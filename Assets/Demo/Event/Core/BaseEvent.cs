using System;
using System.Collections.Generic;
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

    // 方法映射表（无反射）
    private readonly Dictionary<string, Action<object>> _methodMap = new();
    #endregion
    protected BaseEvent()
    {
        EventId = Guid.NewGuid().ToString("N");
    }
    /// <summary>
    /// 根据接收到的参数执行方法
    /// </summary>
    /// <param name="funcKey"></param>
    /// <param name="data"></param>
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
    #region 根据配置绑定方法
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
        foreach (var binding in _config.bindings)
        {
            if (!binding.enable || string.IsNullOrEmpty(binding.webSendToUnityFuncName))
                continue;

            var method = type.GetMethod(
                binding.unityFuncName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic
            );

            if (method == null)
            {
                Debug.LogError($"找不到方法：{binding.unityFuncName} 在 {type.Name}");
                continue;
            }

            Action<object> action = data => method.Invoke(this, new[] { data });
            _methodMap[binding.webSendToUnityFuncName] = action;
        }
    }
    #endregion
}