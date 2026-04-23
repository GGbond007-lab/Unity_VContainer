using System;
using Cysharp.Threading.Tasks;

namespace VContainer.StateMachine
{
    public interface IState
    {
        event Action OnEnter;
        event Action OnUpdate;
        event Action OnExit;
        
        void Enter();
        void Update();
        void Exit();
        
        UniTask EnterAsync();
        UniTask UpdateAsync();
        UniTask ExitAsync();
    }
}