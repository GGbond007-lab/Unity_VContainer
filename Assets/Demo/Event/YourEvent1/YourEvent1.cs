using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class YourEvent1 : BaseEvent
{
    private readonly ILabelManager _labelManager;
    private readonly YourEvent1LabelController _labelController;

    public YourEvent1(ILabelManager labelManager, YourEvent1LabelController labelController)
    {
        _labelManager = labelManager;
        _labelController = labelController;
    }

    protected override void RegisterLabelController()
    {
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
            // 🔥 用 identifyID 查找
            var existLabel = LabelCtrl.TryGetLabel(itemData.identifyID);
            if (existLabel != null)
            {
                existLabel.SetData(itemData);
                existLabel.Refresh();
                continue;
            }

            var prefab = await _labelManager.LoadLabelPrefab(itemData.prefabKey);
            var newLabel = _labelManager.CreateLabel(prefab, LabelCtrl.RootTransform);

            newLabel.SetData(itemData);
            LabelCtrl.AddLabel(newLabel);
        }
    }

    public void PrintYourEvent1()
    {
        Debug.Log($"【YourEvent1：Print1】:{EventId}");
    }

    public void ShowData(object data)
    {
        Debug.Log($"【YourEvent1 展示数据】：{data}");
    }

    public void OnCallBackSpawnLabelList()
    {
        Debug.Log("OnCallBackSpawnLabelList回调方法被调用了！");
    }
}