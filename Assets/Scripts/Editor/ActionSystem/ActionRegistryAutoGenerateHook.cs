#if UNITY_EDITOR
using UniVCon;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UniVCon.Editor
{
    [InitializeOnLoad]
    public sealed class ActionRegistryAutoGenerateHook : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        static ActionRegistryAutoGenerateHook()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            ThrowIfValidationFails();
            ActionRegistryGenerator.GenerateSilently();
            Debug.Log("[ActionRegistryAutoGenerateHook] Generated ActionRegistry before build.");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            LogValidationErrors();
            ActionRegistryGenerator.GenerateSilently();
            Debug.Log("[ActionRegistryAutoGenerateHook] Generated ActionRegistry before entering Play Mode.");
        }

        private static void LogValidationErrors()
        {
            foreach (var error in ActionSystemValidator.Validate())
            {
                Debug.LogError($"[ActionSystemValidator] {error}");
            }
        }

        private static void ThrowIfValidationFails()
        {
            var errors = ActionSystemValidator.Validate();
            if (errors.Count == 0)
                return;

            foreach (var error in errors)
            {
                Debug.LogError($"[ActionSystemValidator] {error}");
            }

            throw new BuildFailedException($"Action System validation failed with {errors.Count} error(s).");
        }
    }
}
#endif
