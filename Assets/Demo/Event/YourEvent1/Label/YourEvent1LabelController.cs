using System.Collections.Generic;
using UnityEngine;

public class YourEvent1LabelController : IEventLabelController
{
    private readonly List<LabelItem> _labels = new();
    private readonly Transform _root;
    private readonly GameObject _rootGameObject;

    public YourEvent1LabelController()
    {
        var go = new GameObject("[YourEvent1LabelController]", typeof(RectTransform));
        _rootGameObject = go;
        _root = go.transform;
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
            _root.SetParent(canvas.transform, false);
    }

    public void AddLabel(LabelItem label)
    {
        _labels.Add(label);
        label.transform.SetParent(_root, false);
    }

    public void RemoveLabel(LabelItem label)
    {
        if (_labels.Remove(label))
        {
            Object.Destroy(label.gameObject);
        }
    }

    public void ClearAll()
    {
        foreach (var label in _labels)
            Object.Destroy(label.gameObject);
        _labels.Clear();
    }

    public void Destroy()
    {
        ClearAll();
        if (_rootGameObject != null)
            Object.Destroy(_rootGameObject);
    }
    public LabelItem TryGetLabel(string deviceName)
    {
        foreach (var item in _labels)
        {
            var data = item._typedData;
            if (data != null && data.deviceName == deviceName)
            {
                return item;
            }
        }
        return null;
    }
}

