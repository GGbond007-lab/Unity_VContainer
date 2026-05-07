#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ActionSystem.Editor
{
    public sealed class ActionRegistryAssetPostprocessor : AssetPostprocessor
    {
        private static bool _repairQueued;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!ShouldRepair(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths))
                return;

            QueueRepair();
        }

        private static bool ShouldRepair(params string[][] assetGroups)
        {
            return assetGroups
                .Where(group => group != null)
                .SelectMany(group => group)
                .Any(IsRelevantPath);
        }

        private static bool IsRelevantPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            var normalized = path.Replace('\\', '/');
            if (normalized == ActionRegistryGenerator.OutputPath)
                return false;

            return normalized.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                   normalized.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) &&
                   (normalized.Contains("/ActionConfigs/", StringComparison.OrdinalIgnoreCase) ||
                    normalized.Contains("/AddressableAssetsData/", StringComparison.OrdinalIgnoreCase));
        }

        private static void QueueRepair()
        {
            if (_repairQueued)
                return;

            _repairQueued = true;
            EditorApplication.delayCall += Repair;
        }

        private static void Repair()
        {
            _repairQueued = false;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                QueueRepair();
                return;
            }

            ActionRegistryGenerator.GenerateSilently();
            foreach (var issue in ActionSystemValidator.ValidateIssues())
            {
                var message = $"[ActionSystemValidator] {issue.Message}";
                if (issue.Severity == ActionValidationSeverity.Error)
                    Debug.LogError(message);
                else
                    Debug.LogWarning(message);
            }
        }
    }
}
#endif
