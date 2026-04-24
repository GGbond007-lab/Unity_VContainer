/// <summary>
/// 消息发送器接口（解耦核心：所有发送都依赖它，不依赖具体实现）
/// </summary>
public interface IMessageSender
{
    // 发送【指定Action】消息（WebMessageEFD）
    void SendActionMessage(string type, string actionName, string funcName, object data);

    // 发送【当前上下文】消息（WebMessageFD）
    void SendCurrentMessage(string type, string funcName, object data);
}