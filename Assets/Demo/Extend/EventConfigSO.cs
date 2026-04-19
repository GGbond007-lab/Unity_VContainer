using System;
using System.Collections.Generic;
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

    //[Header("回调设置")]
    //public bool needCallback;
    //public string callbackFuncName;
    //public string[] returnFields;
}

[Serializable]
public class EventMethodBind
{
    public bool enable = true;
    public string webFuncName;
    public string unityFuncName;    
    public bool callBackEnable = false;
    public string callBackFuncName; // 存回调方法名（用于快速选择）
}