#if UNITY_EDITOR
namespace UniVCon.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
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
            if (prop == null || prop.objectReferenceValue == null)
                return;

            if (prop.objectReferenceValue is not MonoScript script)
                return;

            var selfType = script.GetClass();
            if (selfType == null)
                return;

            DrawMethodBindTools(selfType);
            DrawSubscribeTools(selfType);
        }

        private void DrawMethodBindTools(Type actionType)
        {
            GUILayout.Space(8);
            EditorGUILayout.LabelField("Method Binding Tools", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Only UniTask methods with zero parameters or one object parameter can be selected.",
                MessageType.Info);

            for (var i = 0; i < _config.methodBinds.Count; i++)
            {
                var bind = _config.methodBinds[i];
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField($"Method bind [{i}]", EditorStyles.miniBoldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select WebCallable Method", GUILayout.Height(24)))
                {
                    ShowActionMethodMenu(actionType, "Select WebCallable Method", IsValidActionMethod, mi =>
                    {
                        bind.unityFuncName = mi.Name;
                        SaveRefresh();
                    });
                }

                if (GUILayout.Button("Clear", GUILayout.Width(60), GUILayout.Height(24)))
                {
                    bind.unityFuncName = string.Empty;
                    SaveRefresh();
                }

                EditorGUILayout.EndHorizontal();

                if (bind.callBackEnable)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Select Callback Method", GUILayout.Height(24)))
                    {
                        ShowActionMethodMenu(actionType, "Select Callback Method", IsValidActionMethod, mi =>
                        {
                            bind.callBackFuncName = mi.Name;
                            SaveRefresh();
                        });
                    }

                    if (GUILayout.Button("Clear", GUILayout.Width(60), GUILayout.Height(24)))
                    {
                        bind.callBackFuncName = string.Empty;
                        SaveRefresh();
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSubscribeTools(Type selfType)
        {
            GUILayout.Space(12);
            EditorGUILayout.LabelField("Action Subscription Tools", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Target methods must be WebCallable. Local subscribe methods must return UniTask and accept zero parameters, object, or ActionMethodExecutedMessage.",
                MessageType.Info);

            for (var i = 0; i < _config.subscribeBinds.Count; i++)
            {
                var sub = _config.subscribeBinds[i];
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField($"Subscribe bind [{i}]", EditorStyles.miniBoldLabel);

                if (GUILayout.Button("Select Target Action", GUILayout.Height(26)))
                    ShowActionClassMenu(sub);

                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(sub.targetActionClassName)))
                {
                    if (GUILayout.Button("Select Target Action Method", GUILayout.Height(26)))
                    {
                        var targetType = FindActionType(sub.targetActionClassName);
                        if (targetType != null)
                        {
                            ShowActionMethodMenu(targetType, "Select Target Action Method", IsValidActionMethod, mi =>
                            {
                                sub.methodNameInTargetAction = mi.Name;
                                SaveRefresh();
                            });
                        }
                    }
                }

                if (GUILayout.Button("Select Local Subscribe Method", GUILayout.Height(26)))
                {
                    ShowActionMethodMenu(selfType, "Select Local Subscribe Method", IsValidSubscribeMethod, mi =>
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
            var menu = new GenericMenu();
            foreach (var type in FindActionTypes())
            {
                menu.AddItem(new GUIContent(type.FullName), false, () =>
                {
                    sub.targetActionClassName = type.FullName;
                    SaveRefresh();
                });
            }

            if (menu.GetItemCount() == 0)
                EditorUtility.DisplayDialog("No Actions Found", "No runtime Action types are available.", "OK");
            else
                menu.ShowAsContext();
        }

        private static void ShowActionMethodMenu(
            Type actionType,
            string title,
            Func<MethodInfo, bool> predicate,
            Action<MethodInfo> onSelected)
        {
            var menu = new GenericMenu();
            foreach (var method in actionType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!predicate(method))
                    continue;

                menu.AddItem(new GUIContent($"{method.Name} {FormatSignature(method)}"), false, () => onSelected(method));
            }

            if (menu.GetItemCount() == 0)
                EditorUtility.DisplayDialog("No Methods Found", $"{title}: no valid methods were found.", "OK");
            else
                menu.ShowAsContext();
        }

        private static bool IsValidActionMethod(MethodInfo method)
        {
            if (method == null || method.IsSpecialName || method.ReturnType != typeof(UniTask))
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 0 ||
                   parameters.Length == 1 && parameters[0].ParameterType == typeof(object);
        }

        private static bool IsValidSubscribeMethod(MethodInfo method)
        {
            if (method == null || method.IsSpecialName || method.ReturnType != typeof(UniTask))
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 0 ||
                   parameters.Length == 1 &&
                   (parameters[0].ParameterType == typeof(object) ||
                    parameters[0].ParameterType == typeof(ActionMethodExecutedMessage));
        }

        private static string FormatSignature(MethodInfo method)
        {
            var parameters = method.GetParameters();
            return parameters.Length == 0
                ? "()"
                : $"({parameters[0].ParameterType.Name} {parameters[0].Name})";
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
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(pos, prop, label, true);
            GUI.enabled = true;
        }
    }
}
#endif
