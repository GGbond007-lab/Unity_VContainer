using System.Collections.Generic;
using UnityEngine;

public class YourEvent2 : BaseEvent
{
    #region Step1 注入依赖
    private readonly IEventBus _eventBus;
    // 注入 LabelManager 和事件专属的 LabelController（由 VContainer 提供）
    public YourEvent2(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    #endregion

    #region Step3 订阅其他事件方法
    public override void OnInitialize()
    {
        Debug.Log("YourEvent2 初始化，订阅 YourEvent1 的事件");
        _eventBus.Subscribe<YourEvent1>(OnYourEvent1Receive);//订阅事件，参数为事件名和对应的方法
    }
    public void OnYourEvent1Receive(YourEvent1 evt)
    {
        Debug.Log("YourEvent2 收到 YourEvent1 的事件，可以在这里处理 YourEvent1 传递的数据，或者触发 YourEvent2 的标签生成等逻辑");
    }
    #endregion

    public void Print() { Debug.Log(111); }
    public override void OnDestroy()
    {
        _eventBus.UnSubscribe<YourEvent1>(OnYourEvent1Receive);
    }
    
}
