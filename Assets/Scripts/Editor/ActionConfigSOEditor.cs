#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using ActionSystem.Editor;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

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

        var prop = serializedObject.FindProperty("targetActionScript");
        if (prop == null || prop.objectReferenceValue == null) return;

        MonoScript script = prop.objectReferenceValue as MonoScript;
        Type selfType = script.GetClass();
        if (selfType == null) return;

        DrawMethodBindButtonsOnly(selfType);
        DrawSubscribeToolsCorrect(selfType);
    }

    private void DrawMethodBindButtonsOnly(Type actionType)
    {
        GUILayout.Space(8);
        EditorGUILayout.LabelField("方法绑定选择器", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("只显示符合注册表生成规则的方法：返回 UniTask，参数为空或 object。", MessageType.Info);

        for (int i = 0; i < _config.methodBinds.Count; i++)
        {
            var bind = _config.methodBinds[i];
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField($"绑定 [{i}]", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("选择执行方法", GUILayout.Height(24)))
            {
                ShowActionMethodMenu(actionType, "选择 WebCallable 方法", IsValidActionMethod, mi =>
                {
                    bind.unityFuncName = mi.Name;
                    SaveRefresh();
                });
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
                    ShowActionMethodMenu(actionType, "选择 Callback 方法", IsValidActionMethod, mi =>
                    {
                        bind.callBackFuncName = mi.Name;
                        SaveRefresh();
                    });
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

    private void DrawSubscribeToolsCorrect(Type selfType)
    {
        GUILayout.Space(12);
        EditorGUILayout.LabelField("Action订阅（监听其他Action的方法）", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("监听目标方法必须是 WebCallable 形态；本地执行方法必须返回 UniTask，参数为空、object 或 ActionMethodExecutedMessage。", MessageType.Info);

        for (int i = 0; i < _config.subscribeBinds.Count; i++)
        {
            var sub = _config.subscribeBinds[i];
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField($"订阅 [{i}]", EditorStyles.miniBoldLabel);

            if (GUILayout.Button("选择监听目标Action", GUILayout.Height(26)))
            {
                ShowActionClassMenu(sub);
            }

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(sub.targetActionClassName)))
            {
                if (GUILayout.Button("选择目标Action方法", GUILayout.Height(26)))
                {
                    Type targetType = FindActionType(sub.targetActionClassName);
                    if (targetType != null)
                    {
                        ShowActionMethodMenu(targetType, "选择要监听的目标方法", IsValidActionMethod, mi =>
                        {
                            sub.methodNameInTargetAction = mi.Name;
                            SaveRefresh();
                        });
                    }
                }
            }

            if (GUILayout.Button("选择本地执行方法", GUILayout.Height(26)))
            {
                ShowActionMethodMenu(selfType, "选择本地 Subscribe 方法", IsValidSubscribeMethod, mi =>
                {
                    sub.localMethodName = mi.Name;
                    SaveRefresh();
                });
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void ShowActionClassMenu(ActionSubscribeBind sub)
    {
        GenericMenu menu = new GenericMenu();
        foreach (var type in FindActionTypes())
        {
            menu.AddItem(new GUIContent(type.Name), false, () =>
            {
                sub.targetActionClassName = type.FullName;
                SaveRefresh();
            });
        }

        if (menu.GetItemCount() == 0)
            EditorUtility.DisplayDialog("提示", "未找到可用Action", "OK");
        else
            menu.ShowAsContext();
    }

    private void ShowActionMethodMenu(Type actionType, string title, Func<MethodInfo, bool> predicate, Action<MethodInfo> onSelected)
    {
        GenericMenu menu = new GenericMenu();
        foreach (var method in actionType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!predicate(method)) continue;
            menu.AddItem(new GUIContent($"{method.Name} {FormatSignature(method)}"), false, () => onSelected(method));
        }

        if (menu.GetItemCount() == 0)
            EditorUtility.DisplayDialog("提示", $"{title}：没有符合规则的方法", "OK");
        else
            menu.ShowAsContext();
    }

    private static bool IsValidActionMethod(MethodInfo method)
    {
        if (method == null || method.IsSpecialName || method.ReturnType != typeof(UniTask))
            return false;

        var parameters = method.GetParameters();
        return parameters.Length == 0 ||
               (parameters.Length == 1 && parameters[0].ParameterType == typeof(object));
    }

    private static bool IsValidSubscribeMethod(MethodInfo method)
    {
        if (method == null || method.IsSpecialName || method.ReturnType != typeof(UniTask))
            return false;

        var parameters = method.GetParameters();
        return parameters.Length == 0 ||
               (parameters.Length == 1 && (parameters[0].ParameterType == typeof(object) ||
                                           parameters[0].ParameterType == typeof(ActionMethodExecutedMessage)));
    }

    private static string FormatSignature(MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 0)
            return "()";

        return $"({parameters[0].ParameterType.Name} {parameters[0].Name})";
    }

    private static IEnumerable<Type> FindActionTypes()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = Array.FindAll(e.Types, t => t != null);
            }

            foreach (var type in types)
            {
                if (type.IsClass && !type.IsAbstract && typeof(IBaseAction).IsAssignableFrom(type))
                    yield return type;
            }
        }
    }

    private static Type FindActionType(string name)
    {
        foreach (var type in FindActionTypes())
        {
            if (type.Name == name || type.FullName == name)
                return type;
        }
        return null;
    }

    private void SaveRefresh()
    {
        EditorUtility.SetDirty(target);
        serializedObject.Update();
        ActionRegistryGenerator.GenerateSilently();
        Repaint();
    }
}

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
