namespace UniVCon
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    public sealed class AddressableActionConfigProvider : IActionConfigProvider, IDisposable {
        public const string AddressableLabel = "ActionConfig";
        private readonly Dictionary<string, ActionConfigSO> _configByActionName = new();
        private readonly Dictionary<string, ActionConfigSO> _configByTargetType = new();
        private readonly Dictionary<string, ScriptableObject> _scriptableObjectsByKey = new();
        private AsyncOperationHandle<IList<ActionConfigSO>> _loadHandle;
        public bool IsInitialized {
            get;
            private set;
        }
        public async UniTask InitializeAsync() {
            if (IsInitialized) return;
            ReleaseLoadHandle();
            _loadHandle = Addressables.LoadAssetsAsync<ActionConfigSO>(AddressableLabel, RegisterConfig);
            await _loadHandle.ToUniTask();
            IsInitialized = true;
        }
        public void Dispose() {
            ReleaseLoadHandle();
            _configByActionName.Clear();
            _configByTargetType.Clear();
            _scriptableObjectsByKey.Clear();
            IsInitialized = false;
        }
        public IReadOnlyDictionary<string, ActionConfigSO> AllConfigs() {
            return _configByActionName;
        }
        public ActionConfigSO GetConfig(string actionName) {
            if (string.IsNullOrEmpty(actionName)) return null;
            _configByActionName.TryGetValue(actionName, out var config);
            return config;
        }
        public ActionConfigSO GetConfigByTargetAction(Type targetType) {
            if (targetType == null) return null;
            if (_configByTargetType.TryGetValue(targetType.FullName, out var config)) return config;
            _configByTargetType.TryGetValue(targetType.Name, out config);
            return config;
        }
        public ScriptableObject GetScriptableObjectByType(Type soType) {
            if (soType == null || !typeof(ScriptableObject).IsAssignableFrom(soType)) return null;
            foreach (var so in _scriptableObjectsByKey.Values) {
                if (so != null && (so.GetType() == soType || soType.IsAssignableFrom(so.GetType()))) return so;
            }
            return null;
        }
        private void RegisterConfig(ActionConfigSO config) {
            if (config == null) return;
            if (!string.IsNullOrEmpty(config.actionName)) _configByActionName[config.actionName] = config;
            if (!string.IsNullOrEmpty(config.targetActionClassName)) _configByTargetType[config.targetActionClassName] = config;
            if (config.requiredSOs == null) return;
            foreach (var soConfig in config.requiredSOs) {
                if (soConfig?.soReference == null || string.IsNullOrEmpty(soConfig.typeName)) continue;
                _scriptableObjectsByKey[soConfig.typeName] = soConfig.soReference;
            }
        }
        private void ReleaseLoadHandle() {
            if (_loadHandle.IsValid()) Addressables.Release(_loadHandle);
        }
    }
}
