using System;
using Unity.VisualScripting;
using UnityEngine;

public class YourAction2MsgHandler: IActionMsgHandler
{
    #region Step1 定义事件名
    public string ActionName => "你的Action2"; // Step1 定义一个你的事件名要求和 SO 里一致
    #endregion

    #region Step2 注入事件工厂和事件总线
    private readonly Func<Type, object[], IBaseAction> _eventFactory;
    private readonly IActionBus _eventBus;

    public YourAction2MsgHandler(
        Func<Type, object[], IBaseAction> eventFactory,
        IActionBus eventBus)
    {
        _eventFactory = eventFactory;
        _eventBus = eventBus;
    }
    #endregion

    #region Step3 定义一个 Handle 方法，参数可以根据需要自定义
    public void Handle(string funcName, object data)
    {
        // 1. 创建事件（固定流程）
        var evt = _eventFactory(typeof(YourAction2), null) as YourAction2;
        if (evt == null)
            return;

        // 4. 有方法名就执行（不管是不是本次新建的事件）
        if (!string.IsNullOrEmpty(funcName))
        {
            evt.OnExecute(funcName, data);
        }
    }
    #endregion
    
}
