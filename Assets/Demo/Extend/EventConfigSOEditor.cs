#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EventConfigSO))]
public class EventConfigSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认 Inspector（在顶端编辑绑定字段）
        serializedObject.Update();
        base.OnInspectorGUI();

        var config = (EventConfigSO)target;
        if (config.targetEventScript == null)
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }

        var type = config.targetEventScript.GetClass();
        if (type == null)
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("方法绑定 - 选择方法", EditorStyles.boldLabel);

        var methodBindsProp = serializedObject.FindProperty("methodBinds");
        if (methodBindsProp != null)
        {
            for (int i = 0; i < methodBindsProp.arraySize; i++)
            {
                var element = methodBindsProp.GetArrayElementAtIndex(i);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(30));

                // 下方工具仅提供快速选择功能：选择要执行的方法 和 选择回调方法
                if (GUILayout.Button("选择方法", GUILayout.Width(120)))
                {
                    int idx = i;
                    ShowMethodSelector(type, methodName =>
                    {
                        var el = methodBindsProp.GetArrayElementAtIndex(idx);
                        el.FindPropertyRelative("unityFuncName").stringValue = methodName;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    });
                }

                GUILayout.Space(8);

                if (GUILayout.Button("选择回调", GUILayout.Width(120)))
                {
                    int idx = i;
                    ShowMethodSelector(type, methodName =>
                    {
                        var el = methodBindsProp.GetArrayElementAtIndex(idx);
                        el.FindPropertyRelative("callBackFuncName").stringValue = methodName;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }, onlyCallbackMethods: true);
                }

                GUILayout.EndHorizontal();
            }
        }

        // 回调选择已合并到 methodBinds 的每一项中（使用 callBackEnable / callBackFuncName）

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowMethodSelector(Type targetType, System.Action<string> onSelect, bool onlyCallbackMethods = false)
    {
        var menu = new GenericMenu();
        var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            if (method.ReturnType != typeof(void)) continue;
            var parameters = method.GetParameters();
            if (!(parameters.Length == 0 || (parameters.Length == 1 && parameters[0].ParameterType == typeof(object)))) continue;

            var methodName = method.Name;
            if (onlyCallbackMethods && !methodName.StartsWith("OnCallBack")) continue;

            var sig = parameters.Length == 0 ? "()" : "(object)";
            menu.AddItem(new GUIContent(methodName + " " + sig), false, () => onSelect(methodName));
        }

        if (menu.GetItemCount() == 0)
        {
            Debug.LogWarning($"在 {targetType.Name} 中找不到符合条件的方法（void 方法() 或 void 方法(object)）");
            return;
        }

        menu.ShowAsContext();
    }
}
#endif