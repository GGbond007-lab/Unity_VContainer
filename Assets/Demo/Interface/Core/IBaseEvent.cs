public interface IBaseEvent
{
    void OnInitialize(); // 事件初始化
    void OnExecute(string funcKey, object data);    // 事件执行 
    void OnDestroy();    // 事件结束时调用
}