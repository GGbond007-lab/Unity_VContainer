using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class LabelManager : ILabelManager
{
    private readonly Dictionary<string, GameObject> _prefabCache = new();
    private readonly List<GameObject> _globalLabels = new();

    // 加载 Addressable 标签预制体
    public async Task<GameObject> LoadLabelPrefab(string prefabKey)
    {
        if (_prefabCache.TryGetValue(prefabKey, out var prefab))
            return prefab;

        var handle = Addressables.LoadAssetAsync<GameObject>(prefabKey);
        await handle.Task;

        _prefabCache[prefabKey] = handle.Result;
        return handle.Result;
    }

    // 创建标签实例
    public LabelItem CreateLabel(GameObject prefab, Transform parent = null)
    {
        var obj = Object.Instantiate(prefab, parent == null ? null : parent);
        _globalLabels.Add(obj);
        return obj.GetComponent<LabelItem>();
    }

    // 清空全局标签
    public void ClearGlobalLabels()
    {
        foreach (var l in _globalLabels)
            Object.Destroy(l);
        _globalLabels.Clear();
    }
}