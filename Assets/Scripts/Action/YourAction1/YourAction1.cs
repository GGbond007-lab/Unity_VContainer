using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

public class YourAction1 : BaseAction
{
    private readonly YourActionLabelController _labelController;

    public YourAction1(YourActionLabelController labelController)
    {
        _labelController = labelController;
    }

    protected override void RegisterLabelController()
    {
        _labelController.Initialize();
        LabelCtrl = _labelController;
    }

    [Preserve]
    public async UniTask SpawnLabelList(object data)
    {
        if (!TryConvertData(data, out List<LabelData> dataList))
            return;

        if (dataList == null || dataList.Count == 0)
            return;

        if (LabelCtrl == null)
        {
            Debug.LogError("LabelCtrl is not injected.");
            return;
        }

        foreach (var itemData in dataList)
        {
            var existLabel = LabelCtrl.TryGetLabel(itemData.identifyID);
            if (existLabel != null)
            {
                existLabel.SetData(itemData);
                existLabel.Refresh();
                continue;
            }

            var prefab = await LabelManager.LoadLabelPrefab(itemData.prefabKey);
            var newLabel = LabelManager.CreateLabel(prefab, LabelCtrl.RootTransform);

            newLabel.SetData(itemData);
            newLabel.Refresh();
            if (newLabel is LabelItem labelItem)
            {
                labelItem.SetClickEvent(() =>
                {
                    Debug.Log(itemData.deviceName);
                });
            }
            LabelCtrl.AddLabel(newLabel);
        }

        Debug.Log("SpawnLabelList executed.");
    }

    public void OnCallBackSpawnLabelList()
    {
        Debug.Log("OnCallBackSpawnLabelList executed.");
    }

    public void PrintYourAction1()
    {
        Debug.Log($"YourAction1 Print: {ActionId}");
    }

    public void ShowData(object data)
    {
        Debug.Log($"YourAction1 data: {data}");
    }

    [Preserve]
    public UniTask SendCreateLabel(object data)
    {
        var labelList = new List<LabelData>
        {
            new LabelData
            {
                identifyID = "UnityLabel1",
                prefabKey = "",
                title = "Unity label title 1",
                desc = "Unity label desc 1",
                deviceName = "Unity device A"
            }
        };

        MessageSender.SendActionMessage(
            type: "message",
            actionName: "你的Action1",
            funcName: "生成标签",
            data: labelList
        );

        return UniTask.CompletedTask;
    }

    public void SendUpdateLabel(List<LabelData> labelList)
    {
        MessageSender.SendCurrentMessage(
            type: "message",
            funcName: "更新标签",
            data: labelList
        );
    }

    public override void OnInitialize()
    {
        YourAction1SO yourAction1SO = GetSO<YourAction1SO>("ES1的SO");
        Debug.Log($"SO data: Cube={yourAction1SO.Cube}, BoxCollider={yourAction1SO.BoxCollider}, Cube position={yourAction1SO.Cube.transform.position}");
    }
}
