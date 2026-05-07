#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace ActionSystem.Editor
{
    public class ActionCreator : EditorWindow
    {
        private const string MenuPath = "Action System/Create Action";
        private const string ConfigFolderPath = "Assets/Resources/ActionConfigs";

        private string _actionName = string.Empty;

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            GetWindow<ActionCreator>("Create Action");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Create Action", EditorStyles.boldLabel);
            _actionName = EditorGUILayout.TextField("Action Name", _actionName);

            EditorGUILayout.HelpBox(
                "Creates an Action script and ActionConfigSO. Message routing is handled by ActionDispatcher; no MsgHandler file is generated.",
                MessageType.Info);

            if (GUILayout.Button("Create"))
                CreateAction();
        }

        private void CreateAction()
        {
            if (!IsValidIdentifier(_actionName))
            {
                EditorUtility.DisplayDialog("Invalid Action Name", "Use a valid C# class name.", "OK");
                return;
            }

            var folderPath = Path.Combine("Assets", "Scripts", "Action", _actionName);
            var configPath = $"{ConfigFolderPath}/ActionConfig_{_actionName}.asset";
            if (Directory.Exists(folderPath) || File.Exists(configPath))
            {
                EditorUtility.DisplayDialog("Already Exists", "Action folder or config already exists.", "OK");
                return;
            }

            Directory.CreateDirectory(folderPath);
            Directory.CreateDirectory(ConfigFolderPath);

            var actionPath = CreateActionScript(folderPath);
            AssetDatabase.Refresh();

            var actionScript = AssetDatabase.LoadAssetAtPath<MonoScript>(actionPath);
            CreateActionConfig(configPath, actionScript);
            AddConfigToAddressables(configPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ActionRegistryGenerator.Generate();

            _actionName = string.Empty;
            Close();
        }

        private string CreateActionScript(string folderPath)
        {
            var filePath = Path.Combine(folderPath, $"{_actionName}.cs");
            File.WriteAllText(filePath, GetActionTemplate());
            AssetDatabase.ImportAsset(filePath);
            return filePath.Replace("\\", "/");
        }

        private void CreateActionConfig(string configPath, MonoScript actionScript)
        {
            var config = CreateInstance<ActionConfigSO>();
            config.actionName = _actionName;
            config.isLetBack = true;
            config.targetActionClassName = _actionName;

            var serializedConfig = new SerializedObject(config);
            var targetScript = serializedConfig.FindProperty("targetActionScript");
            if (targetScript != null)
                targetScript.objectReferenceValue = actionScript;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();

            config.methodBinds.Add(new ActionMethodBind
            {
                enableWebFunc = true,
                webFuncName = "ExpPing",
                unityFuncName = "ExpPing",
                callBackEnable = true,
                callBackFuncName = "ExpAfterPing"
            });

            AssetDatabase.CreateAsset(config, configPath);
            EditorUtility.SetDirty(config);
        }

        private static void AddConfigToAddressables(string configPath)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;

            settings.AddLabel(AddressableActionConfigProvider.AddressableLabel);
            var guid = AssetDatabase.AssetPathToGUID(configPath);
            var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            entry.address = Path.GetFileNameWithoutExtension(configPath);
            entry.SetLabel(AddressableActionConfigProvider.AddressableLabel, true);
            EditorUtility.SetDirty(settings);
        }

        private string GetActionTemplate()
        {
            return $@"using Cysharp.Threading.Tasks;
using UnityEngine;

public class {_actionName} : BaseAction
{{
    public override void OnInitialize()
    {{
        Debug.Log(""[{_actionName}] initialized."");
    }}

    public UniTask ExpPing()
    {{
        Debug.Log(""[{_actionName}] ExpPing called."");
        return UniTask.CompletedTask;
    }}

    public UniTask ExpAfterPing(object data)
    {{
        MessageSender.SendActionMessage(
            type: ""message"",
            actionName: ""{_actionName}"",
            funcName: ""ExpAfterPing"",
            data: new {{ actionId = ActionId }});

        return UniTask.CompletedTask;
    }}
}}
";
        }

        private static bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (!char.IsLetter(name[0]) && name[0] != '_') return false;

            for (var i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                    return false;
            }

            return true;
        }
    }
}
#endif
