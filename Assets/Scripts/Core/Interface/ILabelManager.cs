using System.Threading.Tasks;
using UnityEngine;

public interface ILabelManager
{
    Task<GameObject> LoadLabelPrefab(string prefabKey);
    ILabel CreateLabel(GameObject prefab, Transform parent = null);
    void ReleaseLabel(Component label);
    void ClearAllLabelsToPool();
    void ClearPoolCompletely();
    void DebugPoolStatus();
}