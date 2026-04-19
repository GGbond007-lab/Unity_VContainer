using System.Collections.Generic;
using UnityEngine;

public class YourEvent1LabelController : IEventLabelController
{
    private readonly List<LabelItem> _labels = new();
    private Transform _root;
    private GameObject _rootGameObject;

    // 🔥 关键：构造函数不再自动创建 GameObject
    public YourEvent1LabelController() { }

    // 🔥 提供手动初始化方法（只有事件需要时才调用）
    public void Initialize()
    {
        if (_rootGameObject != null) return;

        // 只有这里才会创建物体
        var go = new GameObject("[YourEvent1LabelController]", typeof(RectTransform));
        _rootGameObject = go;
        _root = go.transform;

        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
            _root.SetParent(canvas.transform, false);
    }

    public void AddLabel(LabelItem label)
    {
        if (_root == null)
        {
            Debug.LogError("必须先调用 Initialize() 才能使用 LabelController");
            return;
        }
        _labels.Add(label);
        label.transform.SetParent(_root, false);
    }

    public void RemoveLabel(LabelItem label)
    {
        if (_labels.Remove(label))
            Object.Destroy(label.gameObject);
    }

    public void ClearAll()
    {
        foreach (var label in _labels)
            Object.Destroy(label.gameObject);
        _labels.Clear();
    }

    // 🔥 真正的销毁（释放内存）
    public void Destroy()
    {
        ClearAll();
        if (_rootGameObject != null)
        {
            Object.Destroy(_rootGameObject);
            _rootGameObject = null;
            _root = null;
        }
    }

    public LabelItem TryGetLabel(string deviceName)
    {
        foreach (var item in _labels)
        {
            var data = item._typedData;
            if (data != null && data.deviceName == deviceName)
                return item;
        }
        return null;
    }
}