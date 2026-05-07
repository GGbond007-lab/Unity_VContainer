#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ActionSystem.Editor
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
            ActionRegistryGenerator.GenerateSilently();
            Debug.Log("[ActionRegistryAutoGenerateHook] Generated ActionRegistry before build.");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            ActionRegistryGenerator.GenerateSilently();
            Debug.Log("[ActionRegistryAutoGenerateHook] Generated ActionRegistry before entering Play Mode.");
        }
    }
}
#endif
