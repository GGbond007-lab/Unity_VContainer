#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ActionSystem.Editor
{
    public class ActionCreator : EditorWindow
    {
        private string _actionName = string.Empty;
        private const string MenuPath = "Action系统/创建新Action";

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            var window = GetWindow<ActionCreator>("创建Action");
            window.minSize = new Vector2(400, 200);
            window.maxSize = new Vector2(400, 200);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("创建新的Action", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            GUI.backgroundColor = Color.cyan;
            _actionName = EditorGUILayout.TextField("Action名称", _actionName);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "创建后将生成：\n" +
                "• Assets/Scripts/Action/[ActionName]/\n" +
                "• [ActionName].cs (继承BaseAction)\n" +
                "• [ActionName]MsgHandler.cs (继承IActionMsgHandler)",
                MessageType.Info);

            EditorGUILayout.Space(10);

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
            if (GUILayout.Button("创建", GUILayout.Height(30)))
            {
                CreateAction();
            }
            GUI.backgroundColor = Color.white;
        }

        private void CreateAction()
        {
            if (string.IsNullOrWhiteSpace(_actionName))
            {
                EditorUtility.DisplayDialog("错误", "Action名称不能为空！", "确定");
                return;
            }

            if (!IsValidIdentifier(_actionName))
            {
                EditorUtility.DisplayDialog("错误", "Action名称必须是有效的C#类名！\n（不能包含空格或特殊字符）", "确定");
                return;
            }

            string folderPath = Path.Combine("Assets", "Scripts", "Action", _actionName);

            if (Directory.Exists(folderPath))
            {
                EditorUtility.DisplayDialog("错误", $"文件夹已存在：{folderPath}", "确定");
                return;
            }

            try
            {
                Directory.CreateDirectory(folderPath);

                CreateActionScript(folderPath);
                CreateMsgHandlerScript(folderPath);

                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("成功", $"Action '{_actionName}' 创建成功！\n\n路径：{folderPath}", "确定");

                _actionName = string.Empty;
                Close();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"创建失败：{e.Message}", "确定");
            }
        }

        private void CreateActionScript(string folderPath)
        {
            string filePath = Path.Combine(folderPath, $"{_actionName}.cs");
            string content = GetActionTemplate();
            File.WriteAllText(filePath, content);
            AssetDatabase.ImportAsset(filePath);
        }

        private void CreateMsgHandlerScript(string folderPath)
        {
            string filePath = Path.Combine(folderPath, $"{_actionName}MsgHandler.cs");
            string content = GetMsgHandlerTemplate();
            File.WriteAllText(filePath, content);
            AssetDatabase.ImportAsset(filePath);
        }

        private string GetActionTemplate()
        {
            return $@"using UnityEngine;
using VContainer;
using VContainer.Unity;

public class {_actionName} : BaseAction
{{

}}
";
        }

               private string GetMsgHandlerTemplate()
        {
            return $@"using UnityEngine;
using System;

public class {_actionName}MsgHandler : IActionMsgHandler
{{
    public string ActionName => ""{_actionName}"";

    private readonly Func<Type,object[], IBaseAction> _actionFactory;
    private readonly ActionStack _actionStack;

    public {_actionName}MsgHandler(
        Func<Type, object[],IBaseAction> actionFactory,
        ActionStack actionStack)
    {{
        _actionFactory = actionFactory;
        _actionStack = actionStack;
    }}

    public void Handle(string funcName, object data)
    {{
        var currentEvent = _actionStack.GetCurrentAction();

        if (currentEvent != null && currentEvent is {_actionName})
        {{
            currentEvent.OnExecute(funcName, data);
            return;
        }}

        var created = _actionFactory(typeof({_actionName}),null);
        if (created == null)
        {{
            Debug.LogError(""事件工厂返回 null，无法创建 {_actionName}"");
            return;
        }}

        var newEvent = created as {_actionName};
        if (newEvent == null)
        {{
            Debug.LogError($""事件工厂创建的实例不能转换为 {_actionName}，实际类型：{{created.GetType().Name}}"");
            return;
        }}

        if (!string.IsNullOrEmpty(funcName))
        {{
            newEvent.OnExecute(funcName, data);
        }}
    }}
}}
";
        }

        private bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (!char.IsLetter(name[0]) && name[0] != '_') return false;

            for (int i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                    return false;
            }

            return true;
        }
    }
}
#endif
