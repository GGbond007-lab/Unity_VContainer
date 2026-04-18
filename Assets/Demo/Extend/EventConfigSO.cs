using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventConfig_", menuName = "事件系统/事件配置")]
public class EventConfigSO : ScriptableObject
{
    [Header("填写对应前端 eventName")]
    public string eventName;

    [Header("绑定目标事件")]
    public BaseEvent targetEvent;

    [Header("方法别名映射表")]
    public List<EventMethodBinding> bindings = new();

    [ContextMenu("自动扫描可绑定方法")]
    private void AutoScanMethods()
    {
#if UNITY_EDITOR
        if (targetEvent == null)
        {
            Debug.LogWarning("请先绑定 TargetEvent");
            return;
        }

        bindings.Clear();
        var type = targetEvent.GetType();
        var methods = type.GetMethods(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic);

        foreach (var m in methods)
        {
            if (m.ReturnType != typeof(void)) continue;
            var paras = m.GetParameters();
            if (paras.Length != 1 || paras[0].ParameterType != typeof(object))
                continue;

            bindings.Add(new EventMethodBinding
            {
                webSendToUnityFuncName = m.Name,
                unityFuncName = m.Name,
                enable = true
            });
        }

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"✅ 扫描完成：{type.Name} 找到 {bindings.Count} 个可绑定方法");
#endif
    }
}