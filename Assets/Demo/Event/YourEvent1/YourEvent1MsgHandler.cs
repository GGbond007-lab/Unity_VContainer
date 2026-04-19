using System;
using UnityEngine;

public class YourEvent1MsgHandler : IEventMsgHandler
{
    public string EventName => "YourEvent123"; // 和SO里的 eventName 一致

    private readonly Func<Type, IBaseEvent> _eventFactory;
    private readonly IEventBus _eventBus;
    private readonly EventStack _eventStack;
    public YourEvent1MsgHandler(
        Func<Type, IBaseEvent> eventFactory,
        IEventBus eventBus,
        EventStack eventStack)
    {
        _eventFactory = eventFactory;
        _eventBus = eventBus;
        _eventStack = eventStack;
    }

    public void Handle(string funcName, object data)
    {
        // 1. 获取事件栈里 当前正在运行的事件
        var currentEvent = _eventStack.GetCurrentEvent();

        // 2. 判断：栈里已有事件 + 且和当前要创建的事件是【同一个类型】
        if (currentEvent != null && currentEvent is YourEvent1)
        {
            // 🔥 不创建新事件！直接复用当前事件，只执行方法
            currentEvent.OnExecute(funcName, data);
            return;
        }

        // -------------------------------------------------------------------------
        // 下面是：【第一次进入】或【切换事件】→ 正常创建新事件
        // -------------------------------------------------------------------------
        // 创建事件实例（工厂内部会把事件入栈并调用 Initialize）
        var created = _eventFactory(typeof(YourEvent1));
        if (created == null)
        {
            Debug.LogError("事件工厂返回 null，无法创建 YourEvent1");
            return;
        }

        var newEvent = created as YourEvent1;
        if (newEvent == null)
        {
            Debug.LogError($"事件工厂创建的实例不能转换为 YourEvent1，实际类型：{created.GetType().Name}");
            return;
        }

        // 绑定配置
        var config = EventConfigProvider.GetConfig(EventName);
        newEvent.SetConfig(config);

        // 发布事件
        _eventBus.Publish(newEvent);

        // 执行方法
        if (!string.IsNullOrEmpty(funcName))
        {
            newEvent.OnExecute(funcName, data);
        }
    }
}