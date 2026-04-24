using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

/// <summary>
/// 你的Action：自己管理【接收】+【发送】
/// 解耦：只依赖 IMessageSender 接口，不依赖任何具体类
/// </summary>
public class YourAction1 : BaseAction
{
    private readonly ILabelManager _labelManager;
    private readonly YourActionLabelController _labelController;
    // 发送器（VContainer自动注入）
    private readonly IMessageSender _sender;
    public YourAction1(ILabelManager labelManager, YourActionLabelController labelController, IMessageSender sender)
    {
        _labelManager = labelManager;
        _labelController = labelController;
        _sender = sender;
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
        Debug.Log("1111SpawnLabelList方法被调用了！"); // 打印日志，确认方法被调用
    }
    public void OnCallBackSpawnLabelList()
    {
        Debug.Log("OnCallBackSpawnLabelList回调方法被调用了！");
    }
    public void PrintYourEvent1()
    {
        Debug.Log($"【YourEvent1：Print1】:{ActionId}");
    }

    public void ShowData(object data)
    {
        Debug.Log($"【YourEvent1 展示数据】：{data}");
    }



    // Action 自己封装发送方法！
    public void SendCreateLabel()
    {
        List<LabelData> labelList=new List<LabelData>();
        labelList.Add(new LabelData
        {
            identifyID = "Unity返回标签1",
            prefabKey = "",
            title = "Unity返回标签标题1",
            desc = "Unity返回标签描述1",
            deviceName = "Unity返回设备A"
        });
        // Action 自己决定：发什么格式、什么funcName
        _sender.SendActionMessage(
            type: "message",
            actionName: "你的Action1",
            funcName: "生成标签",
            data: labelList
        );
    }

    //当前这个方法和上面那个方法的区别是：上面那个是标记是哪个Action发送的的，下面这个是不带Action标记的，直接发一个funcName，适合一些全局通用的事件
    public void SendUpdateLabel(List<LabelData> labelList)
    {
        _sender.SendCurrentMessage(
            type: "message",
            funcName: "更新标签",
            data: labelList
        );
    }
}