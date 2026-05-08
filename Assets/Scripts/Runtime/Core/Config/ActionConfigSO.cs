namespace UniVCon
{
    using System;
    using System.Collections.Generic;
    using Unity.Collections;
    using UnityEngine;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    [CreateAssetMenu(fileName = "ActionConfig_", menuName = "Action System/Action Config")] public class ActionConfigSO : ScriptableObject {
        [Header("Frontend action name")] public string actionName;
        [Header("Show back button when pushed")] public bool isLetBack;
        [Header("Target Action script (Editor only)")] [SerializeField]
        #if UNITY_EDITOR
        private MonoScript targetActionScript;
        #endif
        [Header("Generated target Action class name")] public string targetActionClassName;
        [Header("Method bindings")] public List<ActionMethodBind> methodBinds = new();
        [Header("Action subscriptions")] public List<ActionSubscribeBind> subscribeBinds = new();
        [Header("Required ScriptableObjects")] public List<ScriptableObjectConfig> requiredSOs = new();
        #if UNITY_EDITOR
        private void OnValidate() {
            if (targetActionScript != null) {
                var type = targetActionScript.GetClass();
                if (type != null) {
                    targetActionClassName = type.FullName;
                }
            }
            else {
                targetActionClassName = string.Empty;
            }
        }
        #endif
    }
    [Serializable] public class ActionMethodBind {
        public bool enableWebFunc = true;
        public string webFuncName;
        [ReadOnly] public string unityFuncName;
        public bool callBackEnable;
        [ReadOnly] public string callBackFuncName;
    }
    [Serializable] public class ActionSubscribeBind {
        [ReadOnly] public string targetActionClassName;
        [ReadOnly] public string methodNameInTargetAction;
        [ReadOnly] public string localMethodName;
    }
    [Serializable] public class ScriptableObjectConfig {
        [Header("Dependency key")] public string typeName;
        [Header("Dependency reference")] public ScriptableObject soReference;
        #if UNITY_EDITOR
        public void OnValidate() {
            if (soReference != null && string.IsNullOrEmpty(typeName)) {
                typeName = soReference.GetType().Name;
            }
        }
        #endif
    }
}
