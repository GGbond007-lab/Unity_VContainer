using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "EventConfig_", menuName = "事件系统/事件配置")]
public class EventConfigSO : ScriptableObject
{
    [Header("前端事件名")]
    public string eventName;

    [Header("目标事件脚本文件")]
    public MonoScript targetEventScript;

    [Header("方法绑定")]
    public List<EventMethodBind> methodBinds = new();

    [Header("事件订阅")]
    public List<EventSubscribeBind> subscribeBinds = new();
}

[Serializable]
public class EventMethodBind
{
    public bool enable = true;
    public string webFuncName;

    [ReadOnly] public string unityFuncName;

    public bool callBackEnable = false;

    [ReadOnly] public string callBackFuncName;
}

[Serializable]
public class EventSubscribeBind
{
    [ReadOnly] public string targetEventClassName;
    [ReadOnly] public string methodNameInTargetEvent;
    [ReadOnly] public string localMethodName;
}