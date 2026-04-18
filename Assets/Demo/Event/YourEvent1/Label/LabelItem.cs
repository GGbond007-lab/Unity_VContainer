using UnityEngine;
using UnityEngine.UI;

public class LabelItem : MonoBehaviour, ILabelItem
{
    
    [Header("基础组件")]
    public Button actionButton;
    public Text title;
    public Text description;
    protected object _data;
    public LabelData _typedData;
    private System.Action _onClick;

    public virtual void SetData(object data)
    {
        _data = data;
        _typedData = data as LabelData;

        UpdateUI();
    }

    // 可选的强类型设置，方便更新 UI
    public void SetData(LabelData data)
    {
        _data = data;
        _typedData = data;
        UpdateUI();
    }

    public void SetClickEvent(System.Action onClick)
    {
        _onClick = onClick;
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => _onClick?.Invoke());
    }

    public virtual void Refresh() { UpdateUI(); }

    protected virtual void UpdateUI()
    {
        if (_typedData == null)
            return;

        if (title != null)
            title.text = _typedData.title;

        if(description != null)
            description.text = _typedData.desc;
    }

    protected virtual void OnDestroy()
    {
        _onClick = null;
    }
}