namespace UniVCon
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    public class LabelManager : ILabelManager {
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _prefabHandles = new();
        private readonly Dictionary<string, GameObject> _prefabCache = new();
        private readonly Dictionary<GameObject, Stack<GameObject>> _labelPool = new();
        private readonly HashSet<GameObject> _activeLabels = new();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();
        public async UniTask<GameObject> LoadLabelPrefab(string prefabKey) {
            if (string.IsNullOrWhiteSpace(prefabKey)) {
                Debug.LogError("[LabelManager] Label prefab key is empty.");
                return null;
            }
            if (_prefabCache.TryGetValue(prefabKey, out var prefab)) return prefab;
            var handle = Addressables.LoadAssetAsync<GameObject>(prefabKey);
            await handle.ToUniTask();
            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null) {
                Debug.LogError($"[LabelManager] Failed to load label prefab: {prefabKey}");
                if (handle.IsValid()) Addressables.Release(handle);
                return null;
            }
            _prefabHandles[prefabKey] = handle;
            _prefabCache[prefabKey] = handle.Result;
            return handle.Result;
        }
        public ILabel CreateLabel(GameObject prefab, Transform parent = null) {
            if (prefab == null) return null;
            var obj = GetOrCreateLabelObject(prefab);
            var label = obj.GetComponent<ILabel>();
            if (label == null) {
                Debug.LogError($"[LabelManager] Prefab '{prefab.name}' does not contain a component implementing ILabel.");
                Object.Destroy(obj);
                return null;
            }
            obj.transform.SetParent(parent, false);
            obj.SetActive(true);
            _instanceToPrefab[obj] = prefab;
            _activeLabels.Add(obj);
            return label;
        }
        public void ReleaseLabel(Component component) {
            if (component == null) return;
            var obj = component.gameObject;
            if (!_activeLabels.Remove(obj)) return;
            obj.transform.SetParent(null, false);
            obj.SetActive(false);
            if (_instanceToPrefab.TryGetValue(obj, out var prefab)) {
                if (!_labelPool.TryGetValue(prefab, out var stack)) {
                    stack = new Stack<GameObject>();
                    _labelPool[prefab] = stack;
                }
                stack.Push(obj);
            }
            else {
                Object.Destroy(obj);
            }
        }
        public void ClearAllLabelsToPool() {
            var tempList = new List<GameObject>(_activeLabels);
            foreach (var obj in tempList) {
                var comp = obj.GetComponent<Component>();
                if (comp != null) ReleaseLabel(comp);
            }
            _activeLabels.Clear();
        }
        public void ClearPoolCompletely() {
            ClearAllLabelsToPool();
            foreach (var stack in _labelPool.Values) {
                foreach (var obj in stack) {
                    Object.Destroy(obj);
                }
            }
            _labelPool.Clear();
            _prefabCache.Clear();
            _instanceToPrefab.Clear();
            foreach (var handle in _prefabHandles.Values) {
                if (handle.IsValid()) Addressables.Release(handle);
            }
            _prefabHandles.Clear();
        }
        public void DebugPoolStatus() {
            var log = "=== Label Pool Status ===\n";
            log += $"Active labels: {_activeLabels.Count}\n";
            log += $"Pooled prefab types: {_labelPool.Count}\n";
            var total = 0;
            foreach (var pair in _labelPool) {
                log += $"{pair.Key.name} -> pooled count: {pair.Value.Count}\n";
                total += pair.Value.Count;
            }
            log += $"Total pooled labels: {total}";
            Debug.Log(log);
        }
        private GameObject GetOrCreateLabelObject(GameObject prefab) {
            if (_labelPool.TryGetValue(prefab, out var stack) && stack.Count > 0) return stack.Pop();
            return Object.Instantiate(prefab);
        }
    }
}
