using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

public class YourAction2 : BaseAction
{
    [Preserve]
    public UniTask Print(object data)
    {
        if (!TryConvertData(data, out List<LabelData> dataList))
        {
            Debug.LogError("Invalid data format.");
            return UniTask.CompletedTask;
        }

        foreach (var itemData in dataList)
        {
            Debug.Log("Success: " + itemData.deviceName);
        }

        return UniTask.CompletedTask;
    }
}
