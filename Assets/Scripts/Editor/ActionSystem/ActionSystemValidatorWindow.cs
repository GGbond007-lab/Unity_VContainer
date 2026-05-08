#if UNITY_EDITOR
using UniVCon;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniVCon.Editor
{
    public sealed class ActionSystemValidatorWindow : EditorWindow
    {
        private readonly List<ActionValidationIssue> _issues = new();
        private Vector2 _scroll;
        private string _summary = "Click Validate to inspect Action configs.";

        [MenuItem("Action System/Validate Configs")]
        public static void Open()
        {
            GetWindow<ActionSystemValidatorWindow>("Action Validator");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate"))
                Validate();

            if (GUILayout.Button("Repair Registry"))
            {
                ActionRegistryGenerator.Generate();
                Validate();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(_summary, MessageType.Info);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var issue in _issues)
            {
                var messageType = issue.Severity == ActionValidationSeverity.Error
                    ? MessageType.Error
                    : MessageType.Warning;
                EditorGUILayout.HelpBox(issue.Message, messageType);
            }
            EditorGUILayout.EndScrollView();
        }

        private void Validate()
        {
            _issues.Clear();
            _issues.AddRange(ActionSystemValidator.ValidateIssues());
            var errorCount = _issues.FindAll(issue => issue.Severity == ActionValidationSeverity.Error).Count;
            var warningCount = _issues.Count - errorCount;
            _summary = errorCount == 0 && warningCount == 0
                ? "No Action system issues found."
                : $"Found {errorCount} errors and {warningCount} warnings.";
        }
    }
}
#endif
