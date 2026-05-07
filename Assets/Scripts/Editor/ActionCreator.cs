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
        private const string ConfigFolderPath = "Assets/Resources/ActionConfigs";

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            var window = GetWindow<ActionCreator>("创建Action");
            window.minSize = new Vector2(460, 250);
            window.maxSize = new Vector2(460, 250);
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
                "• Assets/Scripts/Action/[ActionName]/[ActionName].cs\n" +
                "• Assets/Scripts/Action/[ActionName]/[ActionName]MsgHandler.cs\n" +
                "• Assets/Resources/ActionConfigs/ActionConfig_[ActionName].asset\n" +
                "• Action 内置 Exp 前缀示例方法，并自动生成示例绑定",
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

            string configPath = $"{ConfigFolderPath}/ActionConfig_{_actionName}.asset";
            if (File.Exists(configPath))
            {
                EditorUtility.DisplayDialog("错误", $"配置文件已存在：{configPath}", "确定");
                return;
            }

            try
            {
                Directory.CreateDirectory(folderPath);
                Directory.CreateDirectory(ConfigFolderPath);

                string actionPath = CreateActionScript(folderPath);
                CreateMsgHandlerScript(folderPath);

                AssetDatabase.Refresh();

                var actionScript = AssetDatabase.LoadAssetAtPath<MonoScript>(actionPath);
                CreateActionConfig(configPath, actionScript);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                ActionRegistryGenerator.Generate();

                EditorUtility.DisplayDialog("成功", $"Action '{_actionName}' 创建成功！\n\n路径：{folderPath}\n配置：{configPath}", "确定");

                _actionName = string.Empty;
                Close();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"创建失败：{e.Message}", "确定");
            }
        }

        private string CreateActionScript(string folderPath)
        {
            string filePath = Path.Combine(folderPath, $"{_actionName}.cs");
            string content = GetActionTemplate();
            File.WriteAllText(filePath, content);
            AssetDatabase.ImportAsset(filePath);
            return filePath.Replace("\\", "/");
        }

        private void CreateMsgHandlerScript(string folderPath)
        {
            string filePath = Path.Combine(folderPath, $"{_actionName}MsgHandler.cs");
            string content = GetMsgHandlerTemplate();
            File.WriteAllText(filePath, content);
            AssetDatabase.ImportAsset(filePath);
        }

        private void CreateActionConfig(string configPath, MonoScript actionScript)
        {
            var config = CreateInstance<ActionConfigSO>();
            config.actionName = _actionName;
            config.isLetBack = true;
            config.targetActionClassName = _actionName;

            SerializedObject serializedConfig = new SerializedObject(config);
            SerializedProperty targetScript = serializedConfig.FindProperty("targetActionScript");
            if (targetScript != null)
            {
                targetScript.objectReferenceValue = actionScript;
            }
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();

            config.methodBinds.Add(new ActionMethodBind
            {
                enableWebFunc = true,
                webFuncName = "ExpPing",
                unityFuncName = "ExpPing",
                callBackEnable = true,
                callBackFuncName = "ExpAfterPing"
            });
            config.methodBinds.Add(new ActionMethodBind
            {
                enableWebFunc = true,
                webFuncName = "ExpReceiveData",
                unityFuncName = "ExpReceiveData",
                callBackEnable = true,
                callBackFuncName = "ExpSendDataBack"
            });
            config.methodBinds.Add(new ActionMethodBind
            {
                enableWebFunc = true,
                webFuncName = "ExpCreateLabel",
                unityFuncName = "ExpCreateLabel",
                callBackEnable = false,
                callBackFuncName = string.Empty
            });
            config.methodBinds.Add(new ActionMethodBind
            {
                enableWebFunc = true,
                webFuncName = "ExpClearLabels",
                unityFuncName = "ExpClearLabels",
                callBackEnable = false,
                callBackFuncName = string.Empty
            });

            AssetDatabase.CreateAsset(config, configPath);
            EditorUtility.SetDirty(config);
        }

        private string GetActionTemplate()
        {
            return $@"using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class {_actionName} : BaseAction
{{
    // Exp lifecycle example: called after this Action has been pushed into ActionStack.
    public override void OnInitialize()
    {{
        Debug.Log(""[{_actionName}] Exp OnInitialize."");
    }}

    // Exp web-callable example: no input data.
    public UniTask ExpPing()
    {{
        Debug.Log(""[{_actionName}] ExpPing called."");
        return UniTask.CompletedTask;
    }}

    // Exp web-callable example: receive raw web data and convert it inside Action.
    public UniTask ExpReceiveData(object data)
    {{
        if (TryConvertData(data, out Dictionary<string, object> payload))
        {{
            Debug.Log($""[{_actionName}] ExpReceiveData keys: {{string.Join("", "", payload.Keys)}}"");
        }}
        else
        {{
            Debug.LogWarning(""[{_actionName}] ExpReceiveData failed to convert payload."");
        }}

        return UniTask.CompletedTask;
    }}

    // Exp web-callable example: create labels from frontend data.
    // Expected data shape: List<LabelData>, using prefabKey values configured in Addressables.
    public async UniTask ExpCreateLabel(object data)
    {{
        if (!TryConvertData(data, out List<LabelData> labels) || labels == null)
        {{
            Debug.LogWarning(""[{_actionName}] ExpCreateLabel received invalid label data."");
            return;
        }}

        foreach (var labelData in labels)
        {{
            if (string.IsNullOrEmpty(labelData.prefabKey))
                continue;

            var prefab = await LabelManager.LoadLabelPrefab(labelData.prefabKey);
            var label = LabelManager.CreateLabel(prefab);
            if (label == null)
                continue;

            label.SetData(labelData);
            label.Refresh();
        }}
    }}

    // Exp web-callable example: clear pooled labels.
    public UniTask ExpClearLabels()
    {{
        LabelManager.ClearAllLabelsToPool();
        Debug.Log(""[{_actionName}] ExpClearLabels called."");
        return UniTask.CompletedTask;
    }}

    // Exp callback example: runs after ExpPing if the config enables callback.
    public UniTask ExpAfterPing(object data)
    {{
        Debug.Log(""[{_actionName}] ExpAfterPing callback called."");
        return UniTask.CompletedTask;
    }}

    // Exp callback example: send a message back to frontend after ExpReceiveData.
    public UniTask ExpSendDataBack(object data)
    {{
        MessageSender.SendActionMessage(
            type: ""message"",
            actionName: ""{_actionName}"",
            funcName: ""ExpReceiveDataCallback"",
            data: new
            {{
                actionId = ActionId,
                received = data
            }}
        );

        return UniTask.CompletedTask;
    }}

    // Exp subscribe example: can be selected as local subscribe method in ActionConfigSO.
    public UniTask ExpOnOtherActionExecuted(ActionMethodExecutedMessage message)
    {{
        Debug.Log($""[{_actionName}] Exp observed {{message.Action.GetType().Name}}.{{message.MethodName}}"");
        return UniTask.CompletedTask;
    }}
}}
";
        }

        private string GetMsgHandlerTemplate()
        {
            return $@"using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class {_actionName}MsgHandler : IActionMsgHandler
{{
    public string ActionName => ""{_actionName}"";

    private readonly Func<Type, object[], IBaseAction> _actionFactory;
    private readonly ActionStack _actionStack;

    public {_actionName}MsgHandler(
        Func<Type, object[], IBaseAction> actionFactory,
        ActionStack actionStack)
    {{
        _actionFactory = actionFactory;
        _actionStack = actionStack;
    }}

    public async UniTask Handle(string funcName, object data)
    {{
        var currentAction = _actionStack.GetCurrentAction();

        if (currentAction is {_actionName})
        {{
            await currentAction.OnExecute(funcName, data);
            return;
        }}

        var created = _actionFactory(typeof({_actionName}), null);
        if (created == null)
        {{
            Debug.LogError(""Action factory returned null for {_actionName}."");
            return;
        }}

        if (created is not {_actionName} newAction)
        {{
            Debug.LogError($""Action factory created {{created.GetType().Name}}, expected {_actionName}."");
            return;
        }}

        if (!string.IsNullOrEmpty(funcName))
        {{
            await newAction.OnExecute(funcName, data);
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
