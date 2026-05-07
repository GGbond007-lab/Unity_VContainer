using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface IBaseAction : IDisposable
{
    bool IsDestroyed { get; }
    CancellationToken CancellationToken { get; }

    void OnInitialize();
    void OnPushed();
    UniTask<ActionExecutionResult> OnExecute(string funcKey, object data);
    void OnDestroy();
}
