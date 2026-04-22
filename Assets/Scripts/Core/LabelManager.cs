using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class LabelManager : ILabelManager
{
    // 预制体缓存
    private readonly Dictionary<string, GameObject> _prefabCache = new();

    // 对象池：key = 预制体, value = 池栈
    private readonly Dictionary<GameObject, Stack<GameObject>> _labelPool = new();

    // 当前活跃的标签
    private readonly HashSet<GameObject> _activeLabels = new();

    // 记录：实例对象 → 来源预制体（修复 ReleaseLabel 报错）
    private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

    // 加载 Addressable 预制体
    public async Task<GameObject> LoadLabelPrefab(string prefabKey)
    {
        if (_prefabCache.TryGetValue(prefabKey, out var prefab))
            return prefab;

        var handle = Addressables.LoadAssetAsync<GameObject>(prefabKey);
        await handle.Task;

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

        // 直接获取 ILabel 组件返回
        return obj.GetComponent<ILabel>();
    }

    // 释放标签回池（支持任意 Component）
    public void ReleaseLabel(Component component)
    {
        if (component == null) return;
        GameObject obj = component.gameObject;

        if (!_activeLabels.Contains(obj)) return;
        _activeLabels.Remove(obj);

        // ======================================================================
        // 🔥【超级关键修复】回池必须解除父物体，否则会被旧控制器持有！
        // ======================================================================
        obj.transform.SetParent(null, false);

        // 隐藏
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

    // 清空所有活跃标签 → 全部回池
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

    // 完全清空池（释放内存）
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

    // 调试：打印池状态
    public void DebugPoolStatus()
    {
        string log = "=== 标签对象池状态 ===\n";
        log += $"活跃标签：{_activeLabels.Count}\n";
        log += $"池内预制体类型：{_labelPool.Count}\n";

        int total = 0;
        foreach (var pair in _labelPool)
        {
            log += $"{pair.Key.name} → 池数量：{pair.Value.Count}\n";
            total += pair.Value.Count;
        }

        log += $"池内总闲置：{total}\n";
        Debug.Log(log);
    }
}