// IWebBridge.cs
public interface IWebBridge
{
    void SendMessageToWeb(string json);
    void OnReceiveMessage(string json);

}
