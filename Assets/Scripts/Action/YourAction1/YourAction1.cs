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
            Debug.LogError("[YourAction1] Label controller is not available.");
            return;
        }

        foreach (var itemData in dataList)
        {
            var existingLabel = LabelCtrl.TryGetLabel(itemData.identifyID);
            if (existingLabel != null)
            {
                existingLabel.SetData(itemData);
                existingLabel.Refresh();
                continue;
            }

            var prefab = await LabelManager.LoadLabelPrefab(itemData.prefabKey);
            var newLabel = LabelManager.CreateLabel(prefab, LabelCtrl.RootTransform);
            if (newLabel == null)
                continue;

            newLabel.SetData(itemData);
            newLabel.Refresh();

            if (newLabel is LabelItem labelItem)
            {
                labelItem.SetClickEvent(() => Debug.Log(itemData.deviceName));
            }

            LabelCtrl.AddLabel(newLabel);
        }

        Debug.Log("[YourAction1] SpawnLabelList executed.");
    }

    public void OnCallBackSpawnLabelList()
    {
        Debug.Log("[YourAction1] OnCallBackSpawnLabelList executed.");
    }

    public void PrintYourAction1()
    {
        Debug.Log($"[YourAction1] Print: {ActionId}");
    }

    public void ShowData(object data)
    {
        Debug.Log($"[YourAction1] Data: {data}");
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
            type: WebMsgHandlerManager.MessageType,
            actionName: "YourAction1",
            funcName: "SpawnLabelList",
            data: labelList);

        return UniTask.CompletedTask;
    }

    public void SendUpdateLabel(List<LabelData> labelList)
    {
        MessageSender.SendCurrentMessage(
            type: WebMsgHandlerManager.MessageType,
            funcName: "UpdateLabel",
            data: labelList);
    }

    public override void OnInitialize()
    {
        var yourAction1SO = GetSO<YourAction1SO>();
        if (yourAction1SO == null)
        {
            Debug.LogWarning("[YourAction1] YourAction1SO is not configured.");
            return;
        }

        if (yourAction1SO.CubePrefab == null)
        {
            Debug.LogWarning("[YourAction1] Cube prefab is not configured.");
            return;
        }

        Debug.Log($"[YourAction1] Configured cube prefab: {yourAction1SO.CubePrefab.name}");
    }
}
