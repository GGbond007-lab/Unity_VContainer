using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        List<LabelData> dataList = null;

        if (data is not JArray jArray)
        {
            return;
        }

        dataList = jArray.ToObject<List<LabelData>>();

        // 空值判断
        if (dataList == null || dataList.Count == 0)
            return;


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

            newLabel.SetData(itemData); newLabel.Refresh();
            if (newLabel is LabelItem labelItem)
            {
                // 绑定点击事件，闭包缓存当前 itemData，点击时能直接用
                labelItem.SetClickEvent(() =>
                {
                    Debug.Log(itemData.deviceName);
                });
            }
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