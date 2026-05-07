using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LabelManager : ILabelManager
{
    // 预制体缓存
    private readonly Dictionary<string, GameObject> _prefabCache = new();

    // 对象池：key = 预制体, value = 池栈
    private readonly Dictionary<GameObject, Stack<GameObject>> _labelPool = new();

    // 当前活跃的标签
    private readonly HashSet<GameObject> _activeLabels = new();

    // 记录：实例对象 -> 来源预制体
    private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

    // 加载 Addressable 预制体
    public async UniTask<GameObject> LoadLabelPrefab(string prefabKey)
    {
        if (_prefabCache.TryGetValue(prefabKey, out var prefab))
            return prefab;

        var handle = Addressables.LoadAssetAsync<GameObject>(prefabKey);
        await handle.ToUniTask();

        _prefabCache[prefabKey] = handle.Result;
        return handle.Result;
    }

    public ILabel CreateLabel(GameObject prefab, Transform parent = null)
    {
        if (prefab == null) return null;

        GameObject obj = null;
        if (_labelPool.TryGetValue(prefab, out var stack) && stack.Count > 0)
            obj = stack.Pop();
        else
            obj = Object.Instantiate(prefab);

        obj.transform.SetParent(parent, false);
        obj.SetActive(true);

        _instanceToPrefab[obj] = prefab;
        _activeLabels.Add(obj);

        return obj.GetComponent<ILabel>();
    }

    public void ReleaseLabel(Component component)
    {
        if (component == null) return;
        GameObject obj = component.gameObject;

        if (!_activeLabels.Contains(obj)) return;
        _activeLabels.Remove(obj);

        obj.transform.SetParent(null, false);
        obj.SetActive(false);

        if (_instanceToPrefab.TryGetValue(obj, out var prefab))
        {
            if (!_labelPool.ContainsKey(prefab))
                _labelPool[prefab] = new Stack<GameObject>();

            _labelPool[prefab].Push(obj);
        }
        else
        {
            Object.Destroy(obj);
        }
    }

    public void ClearAllLabelsToPool()
    {
        var tempList = new List<GameObject>(_activeLabels);
        foreach (var obj in tempList)
        {
            var comp = obj.GetComponent<Component>();
            if (comp != null)
                ReleaseLabel(comp);
        }
        _activeLabels.Clear();
    }

    public void ClearPoolCompletely()
    {
        ClearAllLabelsToPool();

        foreach (var stack in _labelPool.Values)
        {
            foreach (var obj in stack)
                Object.Destroy(obj);
        }

        _labelPool.Clear();
        _prefabCache.Clear();
        _instanceToPrefab.Clear();
    }

    public void DebugPoolStatus()
    {
        string log = "=== 标签对象池状态 ===\n";
        log += $"活跃标签：{_activeLabels.Count}\n";
        log += $"池内预制体类型：{_labelPool.Count}\n";

        int total = 0;
        foreach (var pair in _labelPool)
        {
            log += $"{pair.Key.name} -> 池数量：{pair.Value.Count}\n";
            total += pair.Value.Count;
        }

        log += $"池内总闲置：{total}";
        Debug.Log(log);
    }
}
