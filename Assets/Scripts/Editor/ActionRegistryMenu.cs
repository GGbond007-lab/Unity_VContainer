#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ActionSystem.Editor
{
    public static class ActionRegistryMenu
    {
        [MenuItem("Action System/Generate Registry")]
        public static void GenerateRegistry()
        {
            ActionRegistryGenerator.Generate();
        }

        [MenuItem("Action System/Repair Registry")]
        public static void RepairRegistry()
        {
            var issues = ActionRegistryGenerator.RepairAndValidate();
            if (issues.Count == 0)
            {
                Debug.Log("[ActionRegistryMenu] Registry repaired. No validation errors found.");
                return;
            }

            foreach (var issue in issues)
            {
                Debug.LogError($"[ActionRegistryMenu] {issue}");
            }
        }
    }
}
#endif
