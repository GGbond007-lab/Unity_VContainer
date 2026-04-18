using System;

[Serializable]
public class WebMessage
{
    public string eventName;   // 你的事件名："你的方法1"
    public string funcName;   // 你的方法名："打印一下"
    public object data;       // 数据本体 
}