using System;
using UnityEngine;

[Serializable]
public sealed class EventMethodBinding
{
    [Tooltip("前端下发 funcName 别名")]
    public string webSendToUnityFuncName;

    [Tooltip("代码内真实方法名（自动扫描填充）")]
    public string unityFuncName;

    [Tooltip("是否启用该绑定")]
    public bool enable = true;
}