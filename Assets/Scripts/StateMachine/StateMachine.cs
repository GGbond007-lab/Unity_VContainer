using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using VContainer;


public class StateMachine
{
    private readonly IObjectResolver resolver;
    private Dictionary<string, IState> states = new Dictionary<string, IState>();
    private IState currentState;
    private string currentStateName;

    public StateMachine(IObjectResolver resolver)
    {
        this.resolver = resolver;
    }

    public void AddState(string name, IState state)
    {
        states[name] = state;
    }

    public void AddState<T>(string name) where T : IState
    {
        var state = resolver.Resolve<T>();
        states[name] = state;
    }

    public void ChangeState(string name)
    {
        if (states.TryGetValue(name, out var state))
        {
            currentState?.Exit();
            currentState = state;
            currentStateName = name;
            currentState.Enter();
        }
    }

    public async UniTask ChangeStateAsync(string name)
    {
        // If requested state is already current by name, do nothing
        if (currentStateName == name)
        {
            return;
        }

        if (states.TryGetValue(name, out var state))
        {
            // If resolved instance equals current, keep name in sync and return
            if (state == currentState)
            {
                currentStateName = name;
                return;
            }

            if (currentState != null)
            {
                await currentState.ExitAsync();
            }

            currentState = state;
            currentStateName = name;

            await currentState.EnterAsync();
        }
    }

    public void Update()
    {
        currentState?.Update();
    }

    public async UniTask UpdateAsync()
    {
        if (currentState != null)
        {
            await currentState.UpdateAsync();
        }
    }

    public string CurrentStateName => currentStateName;

    public T GetState<T>(string name) where T : IState
    {
        if (states.TryGetValue(name, out var state))
        {
            return (T)state;
        }
        return default;
    }
}
