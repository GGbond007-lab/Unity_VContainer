namespace UniVCon.StateMachine
{
    using System;
    using Cysharp.Threading.Tasks;
    public interface IState {
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
