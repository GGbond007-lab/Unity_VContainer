using System.Collections.Generic;
using UnityEngine;

public class YourEvent1 : BaseEvent
{
    private readonly ILabelManager _labelManager;
    private readonly IEventLabelController _labelController;

    // 注入 LabelManager 和事件专属的 LabelController（由 VContainer 提供）
    public YourEvent1(ILabelManager labelManager, YourEvent1LabelController labelController)
    {
        _labelManager = labelManager;
        _labelController = labelController;
    }
    // 🔥 只有消息触发事件时，才初始化 UI
    public override void OnInitialize()
    {
        base.OnInitialize();

        // ✅ 这里才创建 LabelController 的 GameObject
        _labelController.Initialize();
        LabelCtrl = _labelController;
    }
    public async void SpawnLabelList(object data)
    {
        if (data is not List<LabelData> dataList)
        {
            Debug.LogError("数据格式错误！");
            return;
        }

        if (LabelCtrl == null)
        {
            Debug.LogError("LabelCtrl 未注入");
            return;
        }

        foreach (var itemData in dataList)
        {
            // 🔥 关键：根据唯一ID查找是否已存在
            var existLabel = LabelCtrl.TryGetLabel(itemData.deviceName);
            if (existLabel != null)
            {
                // 已存在：只刷新数据+位置，不生成新物体
                existLabel.SetData(itemData);
                existLabel.Refresh();
                continue;
            }

            // 不存在：正常新建生成
            var prefab = await _labelManager.LoadLabelPrefab(itemData.prefabKey);
            var labelUi = _labelManager.CreateLabel(prefab);

            LabelCtrl.AddLabel(labelUi);
            var labelItem = labelUi.GetComponent<LabelItem>();

            labelItem.SetData(itemData);

            labelItem.SetClickEvent(() =>
            {
                var currentData = labelItem._typedData; // 读取最新数据
                Debug.Log($"点击标签 → ID：{currentData.deviceName}:{currentData.title} 事件：{EventId}");
            });
        }
    }
    public void Print1()
    {
        Debug.Log($"【YourEvent1：Print1】:{EventId}");
        
    }
    public void ShowData(object data)
    {
        Debug.Log($"【YourEvent1 展示数据】：{data}");
    }
    public void OperateLableFunc() 
    {
        
    }

    // 在 VContainer 注入完成后，基类会调用此方法，替换默认的 LabelCtrl
    protected override void RegisterLabelController()
    {
        LabelCtrl = _labelController;
    }

    public void OnCallBackSpawnLabelList() 
    {
        Debug.Log("OnCallBackSpawnLabelList回调方法被调用了！");
    }
}