namespace UniVCon
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    public interface ILabelManager {
        UniTask<GameObject> LoadLabelPrefab(string prefabKey);
        ILabel CreateLabel(GameObject prefab, Transform parent = null);
        void ReleaseLabel(Component label);
        void ClearAllLabelsToPool();
        void ClearPoolCompletely();
        void DebugPoolStatus();
    }
}
