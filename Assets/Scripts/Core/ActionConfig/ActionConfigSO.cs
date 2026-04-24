using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionConfig_", menuName = "Action系统/Action配置")]
public class ActionConfigSO : ScriptableObject
{
    [Header("前端动作名")]
    public string actionName;

    [Header("目标事件脚本文件")]
    public MonoScript targetEventScript;

    [Header("方法绑定")]
    public List<ActionMethodBind> methodBinds = new();

    [Header("Action订阅")]
    public List<ActionSubscribeBind> subscribeBinds = new();
}

[Serializable]
public class ActionMethodBind
{
    public bool enableWebFunc = true;
    public string webFuncName;

    [ReadOnly] public string unityFuncName;

    public bool callBackEnable = false;

    [ReadOnly] public string callBackFuncName;
}

[Serializable]
public class ActionSubscribeBind
{
    [ReadOnly] public string targetActionClassName;
    [ReadOnly] public string methodNameInTargetAction;
    [ReadOnly] public string localMethodName;
}