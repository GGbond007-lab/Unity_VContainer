using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Web 格式消息发送器（实现解耦的发送接口）
/// 职责：仅构造标准格式JSON + 输出/网络发送
/// </summary>
public class WebMessageSender : IMessageSender
{
    /// <summary>
    /// 发送带 actionName 的标准格式（WebMessageEFD）
    /// </summary>
    public void SendActionMessage(string type, string actionName, string funcName, object data)
    {
        var msg = new WebMessageEFD
        {
            type = type,
            actionName = actionName,
            funcName = funcName,
            data = data
        };

        string json = JsonConvert.SerializeObject(msg, Formatting.Indented);
        Debug.Log($"<color=green>[SEND]</color> 发送指定Action：{actionName} | {funcName}\n{json}");

        //WebSocket.Send(json) / 对接TCP/HTTP
    }

    /// <summary>
    /// 发送不带 actionName 的格式（WebMessageFD）
    /// </summary>

    public void SendCurrentMessage(string type, string funcName, object data)
    {
        var msg = new WebMessageFD
        {
            type = type,
            funcName = funcName,
            data = data
        };

        string json = JsonConvert.SerializeObject(msg, Formatting.Indented);
        Debug.Log($"<color=green>[SEND]</color> 发送当前Action：{funcName}\n{json}");

        //WebSocket.Send(json)
    }
}