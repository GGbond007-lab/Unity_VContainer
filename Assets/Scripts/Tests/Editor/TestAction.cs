using System.Threading;
using Cysharp.Threading.Tasks;

public sealed class TestAction : IBaseAction
{
    private readonly CancellationTokenSource _cts = new();
    private readonly CancellationToken _token;

    public bool Destroyed { get; private set; }
    public bool IsDestroyed => Destroyed;
    public CancellationToken CancellationToken => _token;

    public TestAction()
    {
        _token = _cts.Token;
    }

    public void OnInitialize() { }
    public void OnPushed() { }

    public UniTask<ActionExecutionResult> OnExecute(string funcKey, object data)
    {
        return UniTask.FromResult(ActionExecutionResult.Ok(nameof(TestAction), funcKey));
    }

    public void OnDestroy()
    {
        if (Destroyed) return;
        Destroyed = true;
        _cts.Cancel();
        _cts.Dispose();
    }

    public void Dispose()
    {
        OnDestroy();
    }
}
