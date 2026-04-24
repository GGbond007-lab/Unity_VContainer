[System.Serializable]
public class WebMessageEFD//定义前端格式的消息结构，包含事件名、方法名和数据
{
    public string type;
    public string actionName;
    public string funcName;
    public object data;
}
public class WebMessageFD
{
    public string type;
    public string funcName; // 定义前端格式的消息结构，包含方法名和数据，没有事件名
    public object data;
}
