#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets;

namespace ActionSystem.Editor
{
    public enum ActionValidationSeverity
    {
        Error,
        Warning
    }

    public sealed class ActionValidationIssue
    {
        public ActionValidationSeverity Severity { get; }
        public string Message { get; }

        public ActionValidationIssue(ActionValidationSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public override string ToString()
        {
            return $"[{Severity}] {Message}";
        }
    }

    public static class ActionSystemValidator
    {
        public static List<string> Validate()
        {
            return ValidateIssues()
                .Where(issue => issue.Severity == ActionValidationSeverity.Error)
                .Select(issue => issue.Message)
                .ToList();
        }

        public static List<ActionValidationIssue> ValidateIssues()
        {
            var issues = new List<ActionValidationIssue>();
            var configs = LoadActionConfigs();
            var liveActionTypes = FindLiveScriptTypes(t => typeof(IBaseAction).IsAssignableFrom(t));

            ValidateDuplicateActionNames(configs, issues);

            foreach (var config in configs)
            {
                ValidateConfig(config, liveActionTypes, issues);
            }

            ValidateAddressables(configs, issues);
            return issues;
        }

        private static List<ActionConfigSO> LoadActionConfigs()
        {
            return AssetDatabase.FindAssets("t:ActionConfigSO")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ActionConfigSO>)
                .Where(config => config != null)
                .ToList();
        }

        private static void ValidateDuplicateActionNames(IEnumerable<ActionConfigSO> configs, List<ActionValidationIssue> issues)
        {
            foreach (var group in configs.Where(c => !string.IsNullOrEmpty(c.actionName)).GroupBy(c => c.actionName))
            {
                if (group.Count() > 1)
                    AddError(issues, $"Duplicate actionName '{group.Key}': {string.Join(", ", group.Select(c => c.name))}.");
            }
        }

        private static void ValidateConfig(ActionConfigSO config, IReadOnlyList<Type> liveActionTypes, List<ActionValidationIssue> issues)
        {
            if (string.IsNullOrEmpty(config.actionName))
                AddError(issues, $"{config.name}: actionName is empty.");

            var actionType = ResolveConfiguredActionType(config, liveActionTypes, issues);
            if (actionType == null)
                return;

            if (!typeof(IBaseAction).IsAssignableFrom(actionType))
            {
                AddError(issues, $"{config.name}: {actionType.Name} does not implement IBaseAction.");
                return;
            }

            ValidateDuplicateWebFuncNames(config, issues);

            foreach (var bind in config.methodBinds)
            {
                if (!bind.enableWebFunc)
                    continue;

                if (string.IsNullOrEmpty(bind.webFuncName))
                    AddError(issues, $"{config.name}: method bind has empty webFuncName.");

                ValidateActionMethod(actionType, bind.unityFuncName, $"{config.name}: unityFuncName", issues);

                if (bind.callBackEnable)
                    ValidateActionMethod(actionType, bind.callBackFuncName, $"{config.name}: callBackFuncName", issues);
            }

            ValidateSubscribeBinds(config, actionType, liveActionTypes, issues);
        }

        private static void ValidateDuplicateWebFuncNames(ActionConfigSO config, List<ActionValidationIssue> issues)
        {
            foreach (var group in config.methodBinds
                         .Where(bind => bind.enableWebFunc && !string.IsNullOrEmpty(bind.webFuncName))
                         .GroupBy(bind => bind.webFuncName))
            {
                if (group.Count() > 1)
                    AddError(issues, $"{config.name}: duplicate webFuncName '{group.Key}'.");
            }
        }

        private static Type ResolveConfiguredActionType(
            ActionConfigSO config,
            IReadOnlyList<Type> liveActionTypes,
            List<ActionValidationIssue> issues)
        {
            var scriptType = GetTargetActionScriptType(config);
            if (scriptType == null)
            {
                AddError(issues, $"{config.name}: targetActionScript is missing or does not resolve to a class.");
                return null;
            }

            if (!liveActionTypes.Contains(scriptType))
            {
                AddError(issues, $"{config.name}: target script '{scriptType.Name}' is not a live runtime Action script.");
                return null;
            }

            if (!string.IsNullOrEmpty(config.targetActionClassName) &&
                config.targetActionClassName != scriptType.Name &&
                config.targetActionClassName != scriptType.FullName)
            {
                AddWarning(issues, $"{config.name}: targetActionClassName '{config.targetActionClassName}' differs from script type '{scriptType.FullName}'.");
            }

            return scriptType;
        }

        private static void ValidateActionMethod(Type actionType, string methodName, string label, List<ActionValidationIssue> issues)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                AddError(issues, $"{label} is empty.");
                return;
            }

            var method = actionType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                AddError(issues, $"{label} '{methodName}' does not exist on {actionType.Name}.");
                return;
            }

            var parameters = method.GetParameters();
            var valid = method.ReturnType == typeof(UniTask) &&
                        (parameters.Length == 0 ||
                         (parameters.Length == 1 && parameters[0].ParameterType == typeof(object)));

            if (!valid)
                AddError(issues, $"{label} '{methodName}' must return UniTask and accept zero parameters or one object parameter.");
        }

        private static void ValidateSubscribeBinds(
            ActionConfigSO config,
            Type actionType,
            IReadOnlyList<Type> liveActionTypes,
            List<ActionValidationIssue> issues)
        {
            foreach (var sub in config.subscribeBinds)
            {
                var label = $"{config.name}: subscribe bind";
                var targetActionType = ResolveActionTypeName(sub.targetActionClassName, liveActionTypes);
                if (targetActionType == null)
                {
                    AddError(issues, $"{label} targetActionClassName '{sub.targetActionClassName}' does not resolve to a live Action.");
                    continue;
                }

                ValidateActionMethod(targetActionType, sub.methodNameInTargetAction, $"{label} methodNameInTargetAction", issues);
                ValidateSubscribeMethod(actionType, sub.localMethodName, $"{label} localMethodName", issues);
            }
        }

        private static void ValidateSubscribeMethod(Type actionType, string methodName, string label, List<ActionValidationIssue> issues)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                AddError(issues, $"{label} is empty.");
                return;
            }

            var method = actionType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                AddError(issues, $"{label} '{methodName}' does not exist on {actionType.Name}.");
                return;
            }

            var parameters = method.GetParameters();
            var valid = method.ReturnType == typeof(UniTask) &&
                        (parameters.Length == 0 ||
                         (parameters.Length == 1 && (parameters[0].ParameterType == typeof(ActionMethodExecutedMessage) ||
                                                     parameters[0].ParameterType == typeof(object))));

            if (!valid)
                AddError(issues, $"{label} '{methodName}' must return UniTask and accept zero parameters, object, or ActionMethodExecutedMessage.");
        }

        private static Type ResolveActionTypeName(string typeName, IReadOnlyList<Type> liveActionTypes)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            return liveActionTypes.FirstOrDefault(type => type.Name == typeName || type.FullName == typeName);
        }

        private static void ValidateAddressables(IEnumerable<ActionConfigSO> configs, List<ActionValidationIssue> issues)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                AddError(issues, "Addressable settings were not found.");
                return;
            }

            foreach (var config in configs)
            {
                var path = AssetDatabase.GetAssetPath(config);
                var guid = AssetDatabase.AssetPathToGUID(path);
                var entry = settings.FindAssetEntry(guid);
                if (entry == null)
                {
                    AddError(issues, $"{config.name}: not registered as Addressable.");
                    continue;
                }

                if (!entry.labels.Contains(AddressableActionConfigProvider.AddressableLabel))
                    AddError(issues, $"{config.name}: missing Addressables label '{AddressableActionConfigProvider.AddressableLabel}'.");
            }
        }

        private static List<Type> FindLiveScriptTypes(Func<Type, bool> predicate)
        {
            return AssetDatabase.FindAssets("t:MonoScript")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(IsRuntimeScriptPath)
                .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                .Where(script => script != null)
                .Select(script => script.GetClass())
                .Where(type => type != null && type.IsClass && !type.IsAbstract && predicate(type))
                .Distinct()
                .ToList();
        }

        private static bool IsRuntimeScriptPath(string path)
        {
            var normalized = path.Replace('\\', '/');
            return !normalized.Contains("/Editor/", StringComparison.OrdinalIgnoreCase) &&
                   !normalized.Contains("/Tests/Editor/", StringComparison.OrdinalIgnoreCase);
        }

        private static Type GetTargetActionScriptType(ActionConfigSO config)
        {
            var serializedConfig = new SerializedObject(config);
            var targetScript = serializedConfig.FindProperty("targetActionScript");
            return (targetScript?.objectReferenceValue as MonoScript)?.GetClass();
        }

        private static void AddError(List<ActionValidationIssue> issues, string message)
        {
            issues.Add(new ActionValidationIssue(ActionValidationSeverity.Error, message));
        }

        private static void AddWarning(List<ActionValidationIssue> issues, string message)
        {
            issues.Add(new ActionValidationIssue(ActionValidationSeverity.Warning, message));
        }
    }
}
#endif
