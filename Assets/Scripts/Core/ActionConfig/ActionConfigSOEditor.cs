using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
[CustomEditor(typeof(ActionConfigSO))]
public class ActionConfigSOEditor : Editor
{
    private ActionConfigSO _config;

    private void OnEnable()
    {
        _config = (ActionConfigSO)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();

        // 🔥 关键修复：从 serializedObject 中获取 targetEventScript
        var prop = serializedObject.FindProperty("targetEventScript");
        if (prop == null || prop.objectReferenceValue == null) return;

        MonoScript script = prop.objectReferenceValue as MonoScript;
        Type selfType = script.GetClass();
        if (selfType == null) return;

        DrawMethodBindButtonsOnly(selfType);
        DrawSubscribeToolsCorrect(selfType);
    }

    // ====================== 方法绑定 ======================
    private void DrawMethodBindButtonsOnly(Type eventType)
    {
        GUILayout.Space(8);
        EditorGUILayout.LabelField("🔧 方法绑定选择器", EditorStyles.boldLabel);

        for (int i = 0; i < _config.methodBinds.Count; i++)
        {
            var bind = _config.methodBinds[i];
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField($"绑定 [{i}]", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("选择执行方法", GUILayout.Height(24)))
            {
                ShowValidMethodMenu(eventType, bind);
            }
            if (GUILayout.Button("清空", GUILayout.Width(60), GUILayout.Height(24)))
            {
                bind.unityFuncName = string.Empty;
                SaveRefresh();
            }
            EditorGUILayout.EndHorizontal();

            if (bind.callBackEnable)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("选择回调方法", GUILayout.Height(24)))
                {
                    ShowValidMethodMenu(eventType, bind, true);
                }
                if (GUILayout.Button("清空", GUILayout.Width(60), GUILayout.Height(24)))
                {
                    bind.callBackFuncName = string.Empty;
                    SaveRefresh();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
    }

    // ====================== 事件订阅 ======================
    private void DrawSubscribeToolsCorrect(Type selfType)
    {
        GUILayout.Space(12);
        EditorGUILayout.LabelField("📡 事件订阅（监听其他事件的方法）", EditorStyles.boldLabel);

        for (int i = 0; i < _config.subscribeBinds.Count; i++)
        {
            var sub = _config.subscribeBinds[i];
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField($"订阅 [{i}]", EditorStyles.miniBoldLabel);

            // 1. 选择要监听的目标事件
            if (GUILayout.Button("① 选择监听目标事件", GUILayout.Height(26)))
            {
                ShowEventClassMenu(sub);
            }

            // 2. 选择目标事件里的方法
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(sub.targetActionClassName)))
            {
                if (GUILayout.Button("② 选择目标事件方法", GUILayout.Height(26)))
                {
                    Type targetType = FindEventType(sub.targetActionClassName);
                    if (targetType != null) ShowEventMethodMenu(targetType, sub);
                }
            }

            // 3. 选择本地要执行的方法
            if (GUILayout.Button("③ 选择本地执行方法", GUILayout.Height(26)))
            {
                ShowLocalExecuteMethodMenu(selfType, sub);
            }

            EditorGUILayout.EndVertical();
        }
    }

    // 选择监听的事件类
    private void ShowEventClassMenu(ActionSubscribeBind sub)
    {
        GenericMenu m = new GenericMenu();
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var t in asm.GetTypes())
            {
                if (t.IsClass && !t.IsAbstract && typeof(IBaseAction).IsAssignableFrom(t))
                {
                    m.AddItem(new GUIContent(t.Name), false, () =>
                    {
                        sub.targetActionClassName = t.Name;
                        SaveRefresh();
                    });
                }
            }
        }
        m.ShowAsContext();
    }

    private void ShowEventMethodMenu(Type targetType, ActionSubscribeBind sub)
    {
        GenericMenu m = new GenericMenu();
        foreach (var mi in targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            if (mi.ReturnType == typeof(void))
            {
                m.AddItem(new GUIContent(mi.Name), false, () =>
                {
                    sub.methodNameInTargetAction = mi.Name;
                    SaveRefresh();
                });
            }
        }

        if (m.GetItemCount() == 0)
            EditorUtility.DisplayDialog("提示", "目标事件无可用方法", "OK");
        else
            m.ShowAsContext();
    }

    private void ShowLocalExecuteMethodMenu(Type selfType, ActionSubscribeBind sub)
    {
        GenericMenu m = new GenericMenu();
        foreach (var mi in selfType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            if (mi.ReturnType == typeof(void))
            {
                m.AddItem(new GUIContent(mi.Name), false, () =>
                {
                    sub.localMethodName = mi.Name;
                    SaveRefresh();
                });
            }
        }
        m.ShowAsContext();
    }

    private void ShowValidMethodMenu(Type eventType, ActionMethodBind bind, bool isCallback = false)
    {
        GenericMenu m = new GenericMenu();
        foreach (var mi in eventType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            if (mi.ReturnType != typeof(void)) continue;

            m.AddItem(new GUIContent(mi.Name), false, () =>
            {
                if (!isCallback)
                    bind.unityFuncName = mi.Name;
                else
                    bind.callBackFuncName = mi.Name;

                SaveRefresh();
            });
        }
        m.ShowAsContext();
    }

    private Type FindEventType(string name)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var t in asm.GetTypes())
                if (t.Name == name && typeof(IBaseAction).IsAssignableFrom(t))
                    return t;
        return null;
    }

    private void SaveRefresh()
    {
        EditorUtility.SetDirty(target);
        serializedObject.Update();
        Repaint();
    }
}
#endif

// ====================== ReadOnly ======================
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label, true);
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(pos, prop, label, true);
        GUI.enabled = true;
    }
}
#endif