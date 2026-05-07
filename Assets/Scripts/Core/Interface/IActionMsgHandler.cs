using Cysharp.Threading.Tasks;

public interface IActionMsgHandler
{
    string ActionName { get; } // 前端传的 actionName
    UniTask Handle(string funcName, object data);
}
