
// WebBridge.cs
public class WebBridge : IWebBridge
{
    private readonly JsonSerializer _jsonSerializer;
    private  IEventBus _eventBus;

    // 构造注入
    public WebBridge(JsonSerializer jsonSerializer, IEventBus eventBus)
    {
        _jsonSerializer = jsonSerializer;
        _eventBus = eventBus;
    }

    /// <summary>
    /// 发送消息给前端
    /// </summary>
    public void SendMessageToWeb(string json)
    {
        UnityEngine.Debug.Log($"[Web→前端] {json}");
        // 这里对接你的WebGL/JSBridge
    }

    /// <summary>
    /// 接收前端消息
    /// </summary>
    public void OnReceiveMessage(string json)
    {
        UnityEngine.Debug.Log($"[前端→Web] 收到数据：{json}");
        // 解析后发布事件
    }
}