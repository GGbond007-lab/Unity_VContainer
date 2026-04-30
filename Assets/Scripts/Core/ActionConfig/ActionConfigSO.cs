using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ActionConfig_", menuName = "Action系统/Action配置")]
public class ActionConfigSO : ScriptableObject
{
    [Header("前端动作名")]
    public string actionName;

    [Header("是否允许返回")]
    public bool isLetBack = false;

    // ======================== 核心修复 ========================
    [Header("目标事件脚本文件（仅编辑器编辑用）")]
    [SerializeField]
#if UNITY_EDITOR
    private MonoScript targetEventScript;
#endif

    [Header("自动生成的目标类名（运行时使用）")]
    public string targetEventClassName; // 👈 打包后也存在！

    // ==========================================================

    [Header("方法绑定")]
    public List<ActionMethodBind> methodBinds = new();

    [Header("Action订阅")]
    public List<ActionSubscribeBind> subscribeBinds = new();

    [Header("依赖的ScriptableObject")]
    public List<ScriptableObjectConfig> requiredSOs = new();

#if UNITY_EDITOR
    // 编辑器中修改脚本时，自动同步类名
    private void OnValidate()
    {
        if (targetEventScript != null)
        {
            Type type = targetEventScript.GetClass();
            if (type != null)
            {
                targetEventClassName = type.FullName; // 命名空间+类名
            }
        }
        else
        {
            targetEventClassName = string.Empty;
        }
    }
#endif
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

[Serializable]
public class ScriptableObjectConfig
{
    [Header("给你的依赖一个名字")]
    public string typeName;
    
    [Header("依赖的引用")]
    public ScriptableObject soReference;

#if UNITY_EDITOR
    public void OnValidate()
    {
        if (soReference != null && string.IsNullOrEmpty(typeName))
        {
            typeName = soReference.GetType().Name;
        }
    }
#endif
}