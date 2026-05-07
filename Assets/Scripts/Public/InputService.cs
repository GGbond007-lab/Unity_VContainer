using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

public class InputService : IInputService, ITickable
{
    private readonly string test1 = @"
{
    ""type"":""message"",
    ""actionName"": ""你的Action1"",
    ""funcName"": ""生成标签"",
    ""data"": [
        {""identifyID"":""1"",""prefabKey"":""Lable"",""title"":""标签A"",""desc"":""标签A的描述"",""deviceName"":""标签A的设备名""},
        {""identifyID"":""2"",""prefabKey"":""Lable"",""title"":""标签B"",""desc"":""标签B的描述"",""deviceName"":""标签B的设备名""}
    ]
}";

    private readonly List<LabelData> testData1 = new List<LabelData>
    {
        new LabelData
        {
            identifyID = "1",
            prefabKey = "Lable",
            title = "测试设备A",
            desc = "运行状态良好",
            deviceName = "Device_001",
        },
        new LabelData
        {
            identifyID = "2",
            prefabKey = "Lable",
            title = "测试设备B",
            desc = "待机模式",
            deviceName = "Device_002",
        }
    };

    private readonly List<LabelData> testData2 = new List<LabelData>
    {
        new LabelData
        {
            identifyID = "3",
            prefabKey = "Lable",
            title = "测试设备C",
            desc = "运行状态良好",
            deviceName = "Device_001",
        },
        new LabelData
        {
            identifyID = "4",
            prefabKey = "Lable",
            title = "测试设备D",
            desc = "待机模式",
            deviceName = "Device_002",
        }
    };

    private readonly List<LabelData> testData3 = new List<LabelData>
    {
        new LabelData
        {
            identifyID = "5",
            prefabKey = "LableNew",
            title = "测试设备E",
            desc = "运行状态良好",
            deviceName = "Device_003",
        },
        new LabelData
        {
            identifyID = "6",
            prefabKey = "LableNew",
            title = "测试设备F",
            desc = "待机模式",
            deviceName = "Device_004",
        }
    };

    private readonly WebMsgHandlerManager _msgManager;
    private readonly ActionStack _actionStack;
    private readonly ILabelManager _labelManager;

    public InputService(WebMsgHandlerManager msgManager, ActionStack actionStack, ILabelManager labelManager)
    {
        _msgManager = msgManager;
        _actionStack = actionStack;
        _labelManager = labelManager;
        Debug.Log("[InputService] constructed");
    }

    public void Tick() => CheckInput();

    public void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _msgManager.ReceiveMessageFromWeb(test1).Forget();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _actionStack.Pop();
            _labelManager.DebugPoolStatus();
        }
    }
}
