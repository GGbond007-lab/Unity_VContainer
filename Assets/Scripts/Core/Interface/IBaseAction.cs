using Cysharp.Threading.Tasks;

public interface IBaseAction
{
    void OnInitialize(); // 事件初始化
    void OnPushed();     // 入栈完成后调用
    UniTask OnExecute(string funcKey, object data);    // 事件执行
    void OnDestroy();    // 事件结束时调用
}
