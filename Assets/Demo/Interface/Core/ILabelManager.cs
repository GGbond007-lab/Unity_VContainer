using System.Threading.Tasks;
using UnityEngine;

public interface ILabelManager
{
    Task<GameObject> LoadLabelPrefab(string prefabKey);
    LabelItem CreateLabel(GameObject prefab, Transform parent = null);
    void ClearGlobalLabels();
}