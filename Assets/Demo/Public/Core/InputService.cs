using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

public class InputService : IInputService, ITickable
{

    #region 定义测试参数
    // 构建前端格式的真实 data 数据
    List<LabelData> testData1 = new List<LabelData>
    {
        new LabelData
        {
            identifyID="1",
            prefabKey = "Lable",
            title = "测试设备A",
            desc = "运行状态良好",
            deviceName = "Device_001",
        },
        new LabelData
        {
            identifyID="2",
            prefabKey = "Lable",
            title = "测试设备B",
            desc = "待机模式",
            deviceName = "Device_002",
        }
    };
    List<LabelData> testData2 = new List<LabelData>
    {
        new LabelData
        {
            identifyID="3",
            prefabKey = "Lable",
            title = "测试设备C",
            desc = "运行状态良好",
            deviceName = "Device_001",
        },
        new LabelData
        {
            identifyID="4",
            prefabKey = "Lable",
            title = "测试设备D",
            desc = "待机模式",
            deviceName = "Device_002",
        }
    };
    List<LabelData> testData3 = new List<LabelData>
    {
        new LabelData
        {
            identifyID="5",
            prefabKey = "LableNew",
            title = "测试设备E",
            desc = "运行状态良好",
            deviceName = "Device_003",
        },
        new LabelData
        {
            identifyID="6",
            prefabKey = "LableNew",
            title = "测试设备F",
            desc = "待机模式",
            deviceName = "Device_004",
        }
    };
    #endregion

    private readonly WebMsgHandlerManager _msgManager;
    private readonly EventStack eventStack;
    private readonly ILabelManager _labelManager;
    public InputService(WebMsgHandlerManager msgManager, EventStack eventStack, ILabelManager labelManager)
    {
        _msgManager = msgManager;
        this.eventStack = eventStack;
        _labelManager = labelManager;
        Debug.Log("[InputService] constructed");
    }
    public void Tick() => CheckInput();

    public void CheckInput()
    {
        // debug
        // Debug.Log("[InputService] Tick");
        if (Input.GetKeyDown(KeyCode.Alpha1))//新建事件YoueEvent1，同时新建对应事件和对应方法
        {
            _msgManager.Receive(
               eventName: "YourEvent123",
               funcName: "GetLabel",
               data: testData1
           );
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))//测试前端调用事件方法
        {
            _msgManager.Receive(
               eventName: "YourEvent123",
               funcName: "GetLabel",
               data: testData3
           );
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            eventStack.Pop();
            _labelManager.DebugPoolStatus();
        }
    }


}