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
            prefabKey = "Lable",
            title = "测试设备A",
            desc = "运行状态良好",
            deviceName = "Device_001",
        },
        new LabelData
        {
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
            prefabKey = "Lable",
            title = "测试设备C",
            desc = "运行状态良好",
            deviceName = "Device_001",
        },
        new LabelData
        {
            prefabKey = "Lable",
            title = "测试设备D",
            desc = "待机模式",
            deviceName = "Device_002",
        }
    };
    #endregion

    private readonly WebMsgHandlerManager _msgManager;
    private readonly EventStack eventStack;
    public InputService(WebMsgHandlerManager msgManager, EventStack eventStack)
    {
        _msgManager = msgManager;
        this.eventStack = eventStack;
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
               eventName: "YourEvent2",
               funcName: "打印",
               data: "data2"
           );
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _msgManager.Receive(
               eventName: "YourEvent123",
               funcName: "GetLabel",
               data: testData2
           );
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            eventStack.Pop();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {

        }
    }


}