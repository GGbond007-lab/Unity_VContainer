namespace UniVCon
{
    using System.Collections.Generic;
    using UnityEngine;
    public class YourActionLabelController : IActionLabelController {
        private readonly List<ILabel> _labels = new();
        private readonly ILabelManager _labelManager;
        private Transform _root;
        private GameObject _rootGameObject;
        public YourActionLabelController(ILabelManager labelManager) {
            _labelManager = labelManager;
        }
        public Transform RootTransform => _root;
        public void Initialize() {
            if (_rootGameObject != null) return;
            var go = new GameObject("[EventLabelController]", typeof(RectTransform));
            _rootGameObject = go;
            _root = go.transform;
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null) _root.SetParent(canvas.transform, false);
        }
        public void AddLabel(ILabel label) {
            if (_root == null) Initialize();
            if (label == null) return;
            _labels.Add(label);
            (label as Component).transform.SetParent(_root, false);
        }
        public void RemoveLabel(ILabel label) {
            if (_labels.Remove(label)) _labelManager.ReleaseLabel(label as Component);
        }
        public void ClearAll() {
            foreach (var label in _labels) _labelManager.ReleaseLabel(label as Component);
            _labels.Clear();
        }
        public void Destroy() {
            ClearAll();
            if (_rootGameObject != null) {
                Object.Destroy(_rootGameObject);
                _rootGameObject = null;
                _root = null;
            }
        }
        public ILabel TryGetLabel(string identifyID) {
            foreach (var label in _labels) if (label.identifyID == identifyID) return label;
            return null;
        }
    }
}
