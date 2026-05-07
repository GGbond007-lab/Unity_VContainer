using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YourAction3 : BaseAction
{
    // Exp lifecycle example: called after this Action has been pushed into ActionStack.
    public override void OnInitialize()
    {
        Debug.Log("[YourAction3] Exp OnInitialize.");
    }

    // Exp web-callable example: no input data.
    public UniTask ExpPing()
    {
        Debug.Log("[YourAction3] ExpPing called.");
        return UniTask.CompletedTask;
    }

    // Exp web-callable example: receive raw web data and convert it inside Action.
    public UniTask ExpReceiveData(object data)
    {
        if (TryConvertData(data, out Dictionary<string, object> payload))
        {
            Debug.Log($"[YourAction3] ExpReceiveData keys: {string.Join(", ", payload.Keys)}");
        }
        else
        {
            Debug.LogWarning("[YourAction3] ExpReceiveData failed to convert payload.");
        }

        return UniTask.CompletedTask;
    }

    // Exp web-callable example: create labels from frontend data.
    // Expected data shape: List<LabelData>, using prefabKey values configured in Addressables.
    public async UniTask ExpCreateLabel(object data)
    {
        if (!TryConvertData(data, out List<LabelData> labels) || labels == null)
        {
            Debug.LogWarning("[YourAction3] ExpCreateLabel received invalid label data.");
            return;
        }

        foreach (var labelData in labels)
        {
            if (string.IsNullOrEmpty(labelData.prefabKey))
                continue;

            var prefab = await LabelManager.LoadLabelPrefab(labelData.prefabKey);
            var label = LabelManager.CreateLabel(prefab);
            if (label == null)
                continue;

            label.SetData(labelData);
            label.Refresh();
        }
    }

    // Exp web-callable example: clear pooled labels.
    public UniTask ExpClearLabels()
    {
        LabelManager.ClearAllLabelsToPool();
        Debug.Log("[YourAction3] ExpClearLabels called.");
        return UniTask.CompletedTask;
    }

    // Exp callback example: runs after ExpPing if the config enables callback.
    public UniTask ExpAfterPing(object data)
    {
        Debug.Log("[YourAction3] ExpAfterPing callback called.");
        return UniTask.CompletedTask;
    }

    // Exp callback example: send a message back to frontend after ExpReceiveData.
    public UniTask ExpSendDataBack(object data)
    {
        MessageSender.SendActionMessage(
            type: "message",
            actionName: "YourAction3",
            funcName: "ExpReceiveDataCallback",
            data: new
            {
                actionId = ActionId,
                received = data
            }
        );

        return UniTask.CompletedTask;
    }

    // Exp subscribe example: can be selected as local subscribe method in ActionConfigSO.
    public UniTask ExpOnOtherActionExecuted(ActionMethodExecutedMessage message)
    {
        Debug.Log($"[YourAction3] Exp observed {message.Action.GetType().Name}.{message.MethodName}");
        return UniTask.CompletedTask;
    }
}
